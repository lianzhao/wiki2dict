import { HTTPClient } from './httpClient';

export interface SiteDescription {
  name: string;
  url: string;
}

export interface Site {
  GetDescription(opts?: RequestInit): Promise<SiteDescription>;
}

export class CommonSite extends HTTPClient implements Site {
  constructor(baseUrl: string, fetchOptions?: RequestInit) {
    super(baseUrl, fetchOptions);
  }

  public async GetDescription(opts?: RequestInit) {
    const resp = await this.get('api.php?action=query&meta=siteinfo&format=json', opts);
    return {
      name: resp?.query?.general?.sitename || '',
      url: this.baseUrl,
    };
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
