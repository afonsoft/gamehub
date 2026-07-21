import { CommonModule } from '@angular/common';
import { NgModule } from '@angular/core';
import { LocalizePipe } from '@shared/common/pipes/localize.pipe';

import { ArrayToTreeConverterService } from './array-to-tree-converter.service';
import { AutoFocusDirective } from './auto-focus.directive';
import { BusyIfDirective } from './busy-if.directive';
import { ButtonBusyDirective } from './button-busy.directive';
import { CustomCurrencyPipe } from './custom.currencypipe';
import { FileDownloadService } from './file-download.service';
import { FriendProfilePictureComponent } from './friend-profile-picture.component';
import { LocalStorageService } from './local-storage.service';
import { MomentFormatPipe } from './moment-format.pipe';
import { MomentFromNowPipe } from './moment-from-now.pipe';
import { NullDefaultValueDirective } from './null-value.directive';
import { ScriptLoaderService } from './script-loader.service';
import { StyleLoaderService } from './style-loader.service';
import { TreeDataHelperService } from './tree-data-helper.service';
import { ValidationMessagesComponent } from './validation-messages.component';
import { EqualValidator } from './validation/equal-validator.directive';
import { PasswordComplexityValidator } from './validation/password-complexity-validator.directive';

@NgModule({
  imports: [CommonModule],
  providers: [
    FileDownloadService,
    LocalStorageService,
    ScriptLoaderService,
    StyleLoaderService,
    ArrayToTreeConverterService,
    TreeDataHelperService,
  ],
  declarations: [
    EqualValidator,
    PasswordComplexityValidator,
    ButtonBusyDirective,
    AutoFocusDirective,
    BusyIfDirective,
    MomentFormatPipe,
    MomentFromNowPipe,
    ValidationMessagesComponent,
    NullDefaultValueDirective,
    LocalizePipe,
    CustomCurrencyPipe,
    FriendProfilePictureComponent,
  ],
  exports: [
    EqualValidator,
    PasswordComplexityValidator,
    ButtonBusyDirective,
    AutoFocusDirective,
    BusyIfDirective,
    MomentFormatPipe,
    MomentFromNowPipe,
    ValidationMessagesComponent,
    NullDefaultValueDirective,
    LocalizePipe,
    CustomCurrencyPipe,
    FriendProfilePictureComponent,
  ],
})
export class UtilsModule {}
