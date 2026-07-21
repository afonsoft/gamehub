import { Component, Injector, ViewChild } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { TokenService } from '@eaf/auth/token.service';
import { IAjaxResponse } from '@eaf/eafHttpInterceptor';
import { AppConsts } from '@shared/AppConsts';
import { AppComponentBase } from '@shared/common/app-component-base';
import { ProfileServiceProxy, UpdateProfilePictureInput } from '@shared/service-proxies/service-proxies';
import { ModalDirective } from 'ngx-bootstrap/modal';
import { finalize } from 'rxjs/operators';

@Component({
  standalone: false,
  selector: 'changeProfilePictureModal',
  templateUrl: './change-profile-picture-modal.component.html',
})
export class ChangeProfilePictureModalComponent extends AppComponentBase {
  @ViewChild('changeProfilePictureModal', { static: true }) modal: ModalDirective;

  public active = false;
  public temporaryPictureUrl: string;
  public saving = false;
  private readonly input = new UpdateProfilePictureInput();
  public selectedFile: File | null = null;

  public maxProfilPictureBytesUserFriendlyValue = 50;
  private temporaryPictureFileName: string;

  constructor(
    injector: Injector,
    private readonly _profileService: ProfileServiceProxy,
    private readonly _tokenService: TokenService,
    private readonly _http: HttpClient,
  ) {
    super(injector);
  }

  initializeModal(): void {
    this.active = true;
    this.temporaryPictureUrl = '';
    this.temporaryPictureFileName = '';
    this.selectedFile = null;
  }

  show(): void {
    this.initializeModal();
    this.modal.show();
  }

  close(): void {
    this.active = false;
    this.selectedFile = null;
    this.modal.hide();
  }

  fileChangeEvent(event: any): void {
    const file = event.target.files[0];
    if (file && file.size > 5242880) {
      //5MB
      this.message.warn(this.l('ProfilePicture_Warn_SizeLimit', this.maxProfilPictureBytesUserFriendlyValue));
      return;
    }

    this.selectedFile = file;
  }

  async uploadProfilePicture(): Promise<void> {
    if (!this.selectedFile) {
      this.message.warn(this.l('PleaseSelectAFile'));
      return;
    }

    this.saving = true;
    const formData = new FormData();
    formData.append('file', this.selectedFile);

    try {
      const response = await this._http
        .post<IAjaxResponse>(
          AppConsts.remoteServiceBaseUrl + '/api/services/app/Profile/UploadProfilePicture',
          formData,
          {
            headers: {
              Authorization: 'Bearer ' + this._tokenService.getToken(),
            },
          },
        )
        .toPromise();

      if (response?.success) {
        this.updateProfilePicture(response.result.fileToken);
      } else {
        this.message.error(response?.error?.message || this.l('UploadFailed'));
      }
    } catch (error) {
      (window as any).eaf.log.error(error);
      this.message.error(this.l('UploadFailed'));
    } finally {
      this.saving = false;
    }
  }

  updateProfilePicture(fileToken: string): void {
    this.input.fileToken = fileToken;

    this.saving = true;
    this._profileService
      .updateProfilePicture(this.input)
      .pipe(
        finalize(() => {
          this.saving = false;
        }),
      )
      .subscribe(() => {
        eaf.event.trigger('profilePictureChanged');
        this.close();
      });
  }

  private s4(): string {
    const view = new Uint16Array(1);
    crypto.getRandomValues(view);
    return view[0].toString(16).padStart(4, '0');
  }

  guid(): string {
    return this.s4() + this.s4() + '-' + this.s4() + '-' + this.s4() + '-' + this.s4() + '-' + this.s4() + this.s4() + this.s4();
  }

  save(): void {
    this.uploadProfilePicture();
  }
}
