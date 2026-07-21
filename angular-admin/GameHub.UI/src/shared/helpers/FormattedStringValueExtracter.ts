class ExtractionResult {
  public IsMatch: boolean;
  public Matches: any[];
  public remainingStr: string;

  constructor(isMatch: boolean) {
    this.IsMatch = isMatch;
    this.Matches = [];
  }
}

enum FormatStringTokenType {
  ConstantText,
  DynamicValue,
}

class FormatStringToken {
  public Text: string;

  public Type: FormatStringTokenType;

  constructor(text: string, type: FormatStringTokenType) {
    this.Text = text;
    this.Type = type;
  }
}

class FormatStringTokenizer {
  private tokens: FormatStringToken[];
  private currentText: string;
  private inDynamicValue: boolean;
  private includeBracketsForDynamicValues: boolean;

  Tokenize(format: string, includeBracketsForDynamicValues = false): FormatStringToken[] {
    this.tokens = [];
    this.currentText = '';
    this.inDynamicValue = false;
    this.includeBracketsForDynamicValues = includeBracketsForDynamicValues;

    for (let i = 0; i < format.length; i++) {
      const c = format[i];
      if (c === '{') {
        this.handleOpenBracket(i);
      } else if (c === '}') {
        this.handleCloseBracket(i);
      } else {
        this.currentText += c;
      }
    }

    if (this.inDynamicValue) {
      throw new Error('There is no closing } char for an opened { char.');
    }

    if (this.currentText.length > 0) {
      this.tokens.push(new FormatStringToken(this.currentText, FormatStringTokenType.ConstantText));
    }

    return this.tokens;
  }

  private handleOpenBracket(index: number): void {
    if (this.inDynamicValue) {
      throw new Error('Incorrect syntax at char ' + index + '! format string can not contain nested dynamic value expression!');
    }

    this.inDynamicValue = true;

    if (this.currentText.length > 0) {
      this.tokens.push(new FormatStringToken(this.currentText, FormatStringTokenType.ConstantText));
      this.currentText = '';
    }
  }

  private handleCloseBracket(index: number): void {
    if (!this.inDynamicValue) {
      throw new Error('Incorrect syntax at char ' + index + '! These is no opening brackets for the closing bracket }.');
    }

    this.inDynamicValue = false;

    if (this.currentText.length <= 0) {
      throw new Error('Incorrect syntax at char ' + index + '! Brackets does not containt any chars.');
    }

    let dynamicValue = this.currentText;
    if (this.includeBracketsForDynamicValues) {
      dynamicValue = '{' + dynamicValue + '}';
    }

    this.tokens.push(new FormatStringToken(dynamicValue, FormatStringTokenType.DynamicValue));
    this.currentText = '';
  }
}

export class FormattedStringValueExtracter {
  Extract(str: string, format: string): ExtractionResult {
    if (str === format) {
      return new ExtractionResult(true);
    }

    const formatTokens = new FormatStringTokenizer().Tokenize(format);
    if (!formatTokens) {
      return new ExtractionResult(str === '');
    }

    const result = new ExtractionResult(true);

    for (let i = 0; i < formatTokens.length; i++) {
      const currentToken = formatTokens[i];
      if (currentToken.Type === FormatStringTokenType.ConstantText) {
        const previousToken = i > 0 ? formatTokens[i - 1] : null;
        if (!this.processConstantText(str, currentToken, i === 0, previousToken, result)) {
          return result;
        }
        str = result.remainingStr;
      }
    }

    const lastToken = formatTokens.at(-1);
    if (lastToken.Type === FormatStringTokenType.DynamicValue) {
      result.Matches.push({ name: lastToken.Text, value: str });
    }

    return result;
  }

  private processConstantText(
    str: string,
    token: FormatStringToken,
    isFirstToken: boolean,
    previousToken: FormatStringToken,
    result: ExtractionResult,
  ): boolean {
    if (isFirstToken) {
      if (!str.startsWith(token.Text)) {
        result.IsMatch = false;
        return false;
      }

      result.remainingStr = str.substring(token.Text.length, str.length);
      return true;
    }

    const matchIndex = str.indexOf(token.Text);
    if (matchIndex < 0) {
      result.IsMatch = false;
      return false;
    }

    result.Matches.push({ name: previousToken.Text, value: str.substring(0, matchIndex) });
    result.remainingStr = str.substring(0, matchIndex + token.Text.length);
    return true;
  }

  IsMatch(str: string, format: string): string[] {
    const result = new FormattedStringValueExtracter().Extract(str, format);
    if (!result.IsMatch) {
      return [];
    }

    const values = [];
    for (const match of result.Matches) {
      values.push(match.value);
    }

    return values;
  }
}
