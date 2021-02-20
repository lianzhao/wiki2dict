import { HTTPClient } from './httpClient';
export interface SiteDescription {
  name: string;
  url: string;
}

export interface Page {
  pageid: number;
  ns: number;
  title: string;
}

type apfilterredir = 'all' | 'nonredirects' | 'redirects';

export interface Site {
  getDescription(): Promise<SiteDescription>;
  getAllPages(query?: Record<string, any>): Promise<Page[]>;
  getAllRedirects(query?: Record<string, any>): Promise<Page[]>;
  getPageContent(titles: string[], query?: Record<string, string>): Promise<Record<string, string>>;
}

export class CommonSite extends HTTPClient implements Site {
  constructor(baseUrl: string, fetchOptions?: RequestInit) {
    super(baseUrl, fetchOptions);
  }

  public async getDescription() {
    const resp = await this.get('api.php?action=query&meta=siteinfo&format=json');
    return {
      name: resp?.query?.general?.sitename || '',
      url: this.baseUrl,
    };
  }

  public getAllPages(query?: Record<string, any>) {
    return this.queryAll(gapcontinue => this.queryAllPages({ gapfilterredir: 'nonredirects', ...query, gapcontinue }));
  }

  public getAllRedirects(query?: Record<string, any>) {
    return this.queryAll(async gapcontinue => {
      const resp = await this.queryAllPages({
        pllimit: 'max',
        gapfilterredir: 'redirects',
        prop: 'links',
        ...query,
        gapcontinue,
      });
      if (resp.continue?.plcontinue) {
        console.warn('plcontinue not null, gapcontinue=', gapcontinue, resp);
        // todo
      }
      return resp;
    });
  }

  public async getPageContent(titles: string[], query?: Record<string, string>) {
    const url = this.appendQuery('api.php', {
      action: 'query',
      prop: 'revisions',
      rvslots: '*',
      rvprop: 'content',
      formatversion: 2,
      format: 'json',
      titles: titles.map(encodeURIComponent).join('|'),
      ...query,
    });
    const resp = await this.get(url);
    const result: any = {};
    if (resp.query?.pages) {
      Object.values(resp.query.pages).forEach((page: any) => {
        const title = page.title;
        const content = page.revisions?.[0]?.slots?.main?.content;
        if (title && content) {
          result[title] = content;
        }
      });
    }
    return result;
  }

  protected async queryAll(getPagesFunc: (gapcontinue: string) => Promise<any>) {
    let gapcontinue = '';
    let result: Page[] = [];
    do {
      const resp = await getPagesFunc(gapcontinue);
      gapcontinue = resp.continue?.gapcontinue || '';
      if (resp.query?.pages) {
        result = result.concat(Object.values(resp.query.pages));
      }
    } while (gapcontinue);
    return result;
  }

  protected queryAllPages(query: Record<string, any>) {
    const url = this.appendQuery('api.php', {
      action: 'query',
      generator: 'allpages',
      gapnamespace: 0,
      gaplimit: 'max',
      format: 'json',
      continue: 'gapcontinue||',
      ...query,
    });
    return this.get(url);
  }
}

export class FandomSite extends CommonSite {
  // overwrite
  protected fetch(url: string, opts: RequestInit) {
    return super.fetch(this.appendQuery(url, { origin: '*' }), opts);
  }
}

export function createSite(url: string) {
  const { origin, hostname } = new URL(url);
  if (hostname.includes('.fandom.com')) {
    return new FandomSite(origin);
  }
  return new CommonSite(origin);
}
