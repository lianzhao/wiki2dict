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
  getDescription(opts?: RequestInit): Promise<SiteDescription>;
  getAllPages(query?: Record<string, any>): Promise<Page[]>;
}

export class CommonSite extends HTTPClient implements Site {
  constructor(baseUrl: string, fetchOptions?: RequestInit) {
    super(baseUrl, fetchOptions);
  }

  public async getDescription(opts?: RequestInit) {
    const resp = await this.get('api.php?action=query&meta=siteinfo&format=json', opts);
    return {
      name: resp?.query?.general?.sitename || '',
      url: this.baseUrl,
    };
  }

  public async getAllPages(query?: Record<string, any>) {
    return this.queryAll(gapcontinue => this.query({ gapfilterredir: 'nonredirects', ...query, gapcontinue }));
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

  protected query(query: Record<string, any>) {
    const url = this.appendQuery('api.php', {
      action: 'query',
      generator: 'allpages',
      gapnamespace: 0,
      gaplimit: 'max',
      pllimit: 'max',
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
