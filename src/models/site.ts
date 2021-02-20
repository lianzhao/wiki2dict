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
    const defaultQuery = { apfilterredir: 'nonredirects' };
    let apcontinue = '';
    let result: Page[] = [];
    do {
      const url = this.appendQuery(
        `api.php?action=query&list=allpages&aplimit=max&apcontinue=${apcontinue}&format=json`,
        query || defaultQuery,
      );
      const resp = await this.get(url);
      apcontinue = resp.continue?.apcontinue || '';
      if (resp.query?.allpages) {
        result = result.concat(resp.query?.allpages);
      }
    } while (apcontinue);
    return result;
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
