import { UrlHelper } from './UrlHelper';

describe('UrlHelper', () => {
  it('should have initialUrl defined', () => {
    expect(UrlHelper.initialUrl).toBeDefined();
  });

  it('should parse query parameters from search string', () => {
    const result = UrlHelper.getQueryParametersUsingParameters('?foo=bar&baz=qux');
    expect(result.foo).toBe('bar');
    expect(result.baz).toBe('qux');
  });

  it('should handle empty search string', () => {
    const result = UrlHelper.getQueryParametersUsingParameters('');
    expect(result).toBeDefined();
  });

  it('should get initial url parameters', () => {
    const result = UrlHelper.getInitialUrlParameters();
    expect(result).toBeDefined();
  });

  it('should get return url', () => {
    const result = UrlHelper.getReturnUrl();
    expect(result).toBeDefined();
  });

  it('should get single sign in', () => {
    const result = UrlHelper.getSingleSignIn();
    expect(result).toBeDefined();
  });
});
