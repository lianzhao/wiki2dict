import chunk from 'lodash/chunk';
import parser from 'wtf_wikipedia';
import Zip from 'jszip';
import { DictEntry, formatDict, formatOpf } from './models/dict';
import { createSite } from './models/site';

export interface Message {
  readonly message: string;
  readonly level: string;
  readonly helpLink: string;
}

const chunkSize = 10;
const thumbnailWidth = 300;

function matchHTMLTag(str: string) {
  return str.match(/<[^>]*>/);
}

function getFileExtension(file: string) {
  const index = file.lastIndexOf('.');
  return file.substring(index);
}

export default async function run(
  url: string,
  options?: Partial<{
    langlink: string;
    downloadImage: boolean;
    maxEntries: number;
    onMessage: (msg: Message) => void;
  }>,
) {
  const emitMessage = (message: string, level = 'info', helpLink = '') => {
    options?.onMessage?.({ message, level, helpLink });
  };

  const site = createSite(url);
  const dict: Record<string, DictEntry> = {};
  let progress = 0;
  emitMessage('开始加载基础信息');
  const siteInfo = await site.getDescription();
  emitMessage(`站点名称：${siteInfo.name}`);
  emitMessage('开始加载词条列表');
  let pages = await site.getAllPages();
  if (options?.maxEntries) {
    pages = pages.slice(0, options.maxEntries);
  }
  emitMessage(`共${pages.length}词条`);
  for (const group of chunk(pages, chunkSize)) {
    const contents = await site.getPageContent(group.map(p => p.title));
    for (const key of Object.keys(contents)) {
      const doc = parser(contents[key]);
      const section = doc.sections(0)?.[0]?.text();
      if (!section) {
        emitMessage(`${key}词条获取摘要失败`, 'warn', `${siteInfo.url}/wiki/${key}`);
        continue;
      }
      if (matchHTMLTag(section)) {
        emitMessage(
          `${key}词条摘要中包含HTML tag，这可能是第三方组件的bug`,
          'warn',
          'https://github.com/spencermountain/wtf_wikipedia/issues',
        );
        // console.log(section);
        continue;
      }
      const entry: DictEntry = { key, description: section };
      if (options?.downloadImage) {
        const img = doc.image(0);
        if (img) {
          let fileName = img
            .file()
            .replace('[[', '')
            .replace('File:', '')
            .replace('file:', '')
            .replace('Image:', '')
            .replace(']]', '');
          const index = fileName.indexOf('|'); // xx.jpg|300px
          if (index > 0) {
            fileName = fileName.substring(0, index);
          }
          if (fileName.startsWith('[') || fileName.startsWith('<')) {
            emitMessage(`Invalid file ${fileName} in entry ${entry.key}`, 'debug');
          } else {
            entry.image = fileName;
          }
        }
      }
      dict[key] = entry;
    }
    progress += group.length;
    emitMessage(`已下载${progress}/${pages.length}个词条`);
  }
  emitMessage('开始加载重定向列表');
  const redirects = await site.getAllRedirects();
  emitMessage(`共${pages.length}重定向`);
  const redirectionMap = new Map<string, string>();
  for (const redirect of redirects) {
    const from = redirect.title;
    const to = redirect.links?.[0]?.title || '';
    if (from && to) {
      redirectionMap.set(from, to);
    }
  }
  const findInRedirectionMap = (from: string) => {
    const to = redirectionMap.get(from);
    if (!to) {
      return;
    }
    const to2 = findInRedirectionMap(to);
    return to2 || to; // todo 如果循环重定向……
  };
  for (const redirect of redirects) {
    const from = redirect.title;
    let to = redirect.links?.[0]?.title || '';
    let entry = dict[to];
    if (!entry) {
      to = findInRedirectionMap(from);
      entry = dict[to];
      if (!entry) {
        emitMessage(`目标重定向${to}不存在。源：${from}`, 'warn', `${siteInfo.url}/wiki/${to}`);
        continue;
      }
    }
    if (!entry.alternativeKeys) {
      entry.alternativeKeys = new Set();
    }
    entry.alternativeKeys.add(from);
  }
  if (options?.langlink) {
    emitMessage('开始加载语言链接列表');
    const langlinks = await site.getAllLangLinks(options.langlink);
    emitMessage(`共${langlinks.length}语言链接`);
    for (const langlink of langlinks) {
      const from = langlink.title;
      const to = langlink.langlinks?.[0]['*'];
      const entry = dict[from];
      if (to && entry) {
        if (!entry.alternativeKeys) {
          entry.alternativeKeys = new Set();
        }
        entry.alternativeKeys.add(to);
      } else {
        emitMessage(`目标词条${from}不存在。`, 'warn', `${siteInfo.url}/wiki/${from}`);
      }
    }
  }
  emitMessage('开始针对中文的肮脏处理');
  Object.values(dict).forEach(entry => {
    const tryAdd = (from: string, to: string) => {
      if (from === to) {
        return;
      }
      // emitMessage(`${from} -> ${to}`);
      if (!entry.alternativeKeys) {
        entry.alternativeKeys = new Set();
      }
      entry.alternativeKeys.add(to);
    };
    const keys = [entry.key, ...(entry.alternativeKeys || [])];
    keys.forEach(key => {
      tryAdd(key, key.replaceAll('•', '·'));
      tryAdd(key, key.replaceAll('·', '•'));
    });
  });
  emitMessage('开始生成OPF文件');
  const opf = formatOpf(siteInfo);
  emitMessage('开始生成HTML文件');
  const html = formatDict(siteInfo, Object.values(dict));
  emitMessage('开始打包');
  const zip = new Zip();
  zip.file(`${siteInfo.name}_dict.opf`, opf);
  zip.file(`kindle_dict.html`, html);
  if (options?.downloadImage) {
    const folder = zip.folder('images');
    if (folder) {
      const entries = Object.values(dict).filter(e => e.image);
      let downloaded = 0;
      for (const entry of entries) {
        emitMessage(`img download progress ${downloaded}/${entries.length}`);
        if (!entry.image) {
          continue;
        }
        if (folder.file(entry.image)) {
          emitMessage(`${entry.image} exist, ignore`, 'debug');
        }
        emitMessage(`downloading ${entry.image}`, 'debug');
        const b = await site.downloadFile(entry.image, { thumbnailWidth }).catch(e => {
          emitMessage(`failed to download ${entry.image}, ${e.message}`, 'error');
        });
        if (b) {
          folder.file(entry.image, b);
        }
        downloaded++;
      }
    }
  }
  const data = await zip.generateAsync({ type: 'uint8array' });
  emitMessage('打包完成');
  return { siteInfo, data };
}
