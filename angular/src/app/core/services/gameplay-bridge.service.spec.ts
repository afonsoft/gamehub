import { normalizeChatText } from './gameplay-bridge.service';

describe('normalizeChatText', () => {
  it('removes control characters, normalizes Unicode, and enforces the 500 character limit', () => {
    const input = `Cafe\u0301\u0000${'x'.repeat(600)}`;

    expect(normalizeChatText(input)).toBe(`Café${'x'.repeat(495)}`);
  });
});
