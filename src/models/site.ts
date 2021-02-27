import { HTTPClient } from './httpClient';
export interface SiteInfo {
  name: string;
  url: string;
}

export interface Page {
  pageid: number;
  title: string;
  links?: { title: string }[];
  langlinks?: (Record<'*', string> & { lang: string })[];
}

export interface Site {
  getDescription(): Promise<SiteInfo>;
  getAllPages(query?: Record<string, any>): Promise<Page[]>;
  getAllRedirects(query?: Record<string, any>): Promise<Page[]>;
  getAllLangLinks(lllang: string, query?: Record<string, any>): Promise<Page[]>;
  getPageContent(titles: string[], query?: Record<string, string>): Promise<Record<string, string>>;
}

export class CommonSite extends HTTPClient implements Site {
  private apiPath = '';

  constructor(baseUrl: string, fetchOptions?: RequestInit) {
    super(baseUrl, fetchOptions);
  }

  public async getDescription() {
    await this.ensureApiPath();
    const resp = await this.get(`${this.apiPath}?action=query&meta=siteinfo&format=json`);
    return {
      name: resp?.query?.general?.sitename || '',
      url: this.baseUrl,
    };
  }

  public getAllPages(query?: Record<string, any>) {
    return this.queryAll(gapcontinue =>
      this.queryAllPages({ gapfilterredir: 'nonredirects', ...query, gapcontinue: encodeURIComponent(gapcontinue) }),
    );
  }

  public getAllRedirects(query?: Record<string, any>) {
    return this.queryAll(async gapcontinue => {
      const resp = await this.queryAllPages({
        pllimit: 'max',
        gapfilterredir: 'redirects',
        prop: 'links',
        ...query,
        gapcontinue: encodeURIComponent(gapcontinue),
      });
      if (resp.continue?.plcontinue) {
        console.warn('plcontinue not null, gapcontinue=', gapcontinue, resp);
        // todo
      }
      return resp;
    });
  }

  public getAllLangLinks(lllang: string, query?: Record<string, any>) {
    return this.queryAll(gapcontinue =>
      this.queryAllPages({
        lllimit: 'max',
        gapfilterredir: 'nonredirects',
        gapfilterlanglinks: 'withlanglinks',
        lllang,
        prop: 'langlinks',
        ...query,
        gapcontinue: encodeURIComponent(gapcontinue),
      }),
    );
  }

  public async getPageContent(titles: string[], query?: Record<string, string>) {
    await this.ensureApiPath();
    const url = this.appendQuery(this.apiPath, {
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
        const content = page.revisions?.[0]?.slots?.main?.content || page.revisions?.[0]?.content;
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

  protected async queryAllPages(query: Record<string, any>) {
    await this.ensureApiPath();
    const url = this.appendQuery(this.apiPath, {
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

  protected async ensureApiPath() {
    if (this.apiPath) {
      return;
    }
    const candidates = ['api.php', 'w/api.php'];
    for (const path of candidates) {
      try {
        await this.get(path);
        this.apiPath = path;
        return;
      } catch (e) {
        console.warn(`API path '${path}' 似乎不正确`);
        console.warn(e);
      }
    }
    throw new Error('API path not found');
  }
}

export class FandomSite extends CommonSite {
  // overwrite
  protected fetch(url: string, opts: RequestInit) {
    return super.fetch(this.appendQuery(url, { origin: '*' }), opts);
  }
}

export function createSite(url: string): Site {
  const { origin, hostname } = new URL(url);
  if (hostname.includes('.fandom.com')) {
    return new FandomSite(origin);
  }
  return new CommonSite(origin);
}
