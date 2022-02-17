import merge from 'lodash/merge';
import fetch from 'cross-fetch';
import retry from './retry';

export class HTTPClient {
  protected baseUrl: string;
  protected fetchOptions?: RequestInit;
  protected readonly retry = 3;

  constructor(baseUrl: string, fetchOptions?: RequestInit) {
    this.baseUrl = baseUrl;
    this.fetchOptions = fetchOptions;
  }

  protected get(url: string, opts?: RequestInit) {
    return this.fetch(url, merge({ method: 'GET' }, opts));
  }

  protected fetch(url: string, opts: RequestInit) {
    url = `${this.baseUrl}/${url}`;
    // console.log(url);
    return retry(
      () =>
        fetch(url, merge(this.fetchOptions, opts)).then(resp => {
          if (!resp.ok) {
            const msg = `HTTP response status error (${resp.status}) while sending ${opts.method} request to ${url}`;
            throw new Error(msg);
          }
          return resp;
        }),
      this.retry,
    ).then(resp => {
      const contentType = resp.headers.get('Content-Type');
      return contentType && contentType.toLowerCase().indexOf('json') >= 0 ? resp.json() : resp.text();
    });
  }

  protected appendQuery(url: string, query: Record<string, any>) {
    const queryPart = Object.entries(query)
      .map(([k, v]) => (v === undefined || v === null ? '' : `${k}=${v}`))
      .join('&');
    return `${url}${url.includes('?') ? '&' : '?'}${queryPart}`;
  }
}
