import chunk from 'lodash/chunk';
import parser from 'wtf_wikipedia';
import Zip from 'jszip';
import { DictEntry, formatDict, formatOpf } from './models/dict';
import { createSite, SiteDescription } from './models/site';

export class MessageEvent extends Event {
  public readonly message: string;
  public readonly level: string;
  public readonly helpLink: string;

  constructor(messgae: string, level: string, helpLink = '') {
    super('message');
    this.message = messgae;
    this.level = level;
    this.helpLink = helpLink;
  }
}

export class DoneEvent extends Event {
  public siteInfo: SiteDescription;
  public readonly data: Uint8Array;
  constructor(siteInfo: SiteDescription, data: Uint8Array) {
    super('done');
    this.siteInfo = siteInfo;
    this.data = data;
  }
}

const chunkSize = 10;

export default class Runner extends EventTarget {
  private url: string;

  constructor(url: string) {
    super();
    this.url = url;
  }

  public async run() {
    try {
      const site = createSite(this.url);
      const dict: Record<string, DictEntry> = {};
      let progress = 0;
      this.emitMessage('开始加载基础信息');
      const siteInfo = await site.getDescription();
      this.emitMessage(`站点名称：${siteInfo.name}`);
      this.emitMessage('开始加载词条列表');
      const pages = await site.getAllPages();
      this.emitMessage(`共${pages.length}词条`);
      for (const group of chunk(pages, chunkSize)) {
        const contents = await site.getPageContent(group.map(p => p.title));
        for (const key of Object.keys(contents)) {
          const section = parser(contents[key]).sections(0);
          if (!section) {
            this.emitMessage(`${key}词条获取摘要失败`, 'warn', `${siteInfo.url}/wiki/${key}`);
            continue;
          }
          dict[key] = { key, description: section.text() };
        }
        progress += group.length;
        this.emitMessage(`已下载${progress}/${pages.length}个词条`);
      }
      this.emitMessage('开始加载重定向列表');
      const redirects = await site.getAllRedirects();
      this.emitMessage(`共${pages.length}重定向`);
      for (const redirect of redirects) {
        const from = redirect.title;
        const to = redirect.links?.[0]?.title || '';
        const entry = dict[to];
        if (!entry) {
          this.emitMessage(`目标重定向${to}不存在。源：${from}`, 'warn');
          continue;
        }
        if (entry.alternativeKeys) {
          entry.alternativeKeys.push(from);
        } else {
          entry.alternativeKeys = [from];
        }
      }
      this.emitMessage('开始生成OPF文件');
      const opf = formatOpf(siteInfo);
      this.emitMessage('开始生成HTML文件');
      const html = formatDict(siteInfo, Object.values(dict));
      this.emitMessage('开始打包');
      const zip = new Zip();
      zip.file(`${siteInfo.name}_dict.opf`, opf);
      zip.file(`kindle_dict.html`, html);
      const data = await zip.generateAsync({ type: 'uint8array' });
      this.emitMessage('打包完成');
      this.dispatchEvent(new DoneEvent(siteInfo, data));
    } catch (e) {
      console.error(e);
      this.emitMessage('任务失败', 'error');
      this.dispatchEvent(new ErrorEvent('error', e));
    }
  }

  private emitMessage(msg: string, level = 'info', helpLink = '') {
    this.dispatchEvent(new MessageEvent(msg, level, helpLink));
  }
}
