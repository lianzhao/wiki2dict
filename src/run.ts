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

function matchHTMLTag(str: string) {
  return str.match(/<[^>]*>/);
}

export default async function run(
  url: string,
  options?: Partial<{ langlink: string; onMessage: (msg: Message) => void }>,
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
  const pages = await site.getAllPages();
  emitMessage(`共${pages.length}词条`);
  for (const group of chunk(pages, chunkSize)) {
    const contents = await site.getPageContent(group.map(p => p.title));
    for (const key of Object.keys(contents)) {
      const section = parser(contents[key])
        .sections(0)
        ?.text();
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
      dict[key] = { key, description: section };
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
  const data = await zip.generateAsync({ type: 'uint8array' });
  emitMessage('打包完成');
  return { siteInfo, data };
}
