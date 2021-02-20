import merge from 'lodash/merge';

export class HTTPClient {
  protected baseUrl: string;
  protected fetchOptions?: RequestInit;

  constructor(baseUrl: string, fetchOptions?: RequestInit) {
    this.baseUrl = baseUrl;
    this.fetchOptions = fetchOptions;
  }

  protected get(url: string, opts?: RequestInit) {
    return this.fetch(url, merge({ method: 'GET' }, opts));
  }

  protected fetch(url: string, opts: RequestInit) {
    return fetch(`${this.baseUrl}/${url}`, merge(this.fetchOptions, opts)).then(resp => {
      if (!resp.ok) {
        const msg = `HTTP response status error (${resp.status}) while sending ${opts.method} request to ${url}`;
        throw new Error(msg);
      }
      const contentType = resp.headers.get('Content-Type');
      return contentType && contentType.toLowerCase().indexOf('json') >= 0 ? resp.json() : resp.text();
    });
  }

  protected appendQuery(url: string, query: Record<string, any>) {
    const queryPart = Object.entries(query)
      .map(([k, v]) => `${k}=${v}`)
      .join('&');
    return `${url}${url.includes('?') ? '&' : '?'}${queryPart}`;
  }
}
