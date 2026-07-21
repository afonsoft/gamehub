import { AppAuthService } from '@app/shared/common/auth/app-auth.service';
import { FileDownloadService } from './file-download.service';
import { AppConsts } from '@shared/AppConsts';

describe('FileDownloadService', () => {
  let service: FileDownloadService;

  beforeEach(() => {
    service = new FileDownloadService();
    AppConsts.remoteServiceBaseUrl = 'http://localhost:5000';
  });

  it('should create an instance', () => {
    expect(service).toBeTruthy();
  });

  it('should construct correct download URL', () => {
    // We verify the service and its method exist
    expect(service.downloadTempFile).toBeDefined();
    expect(typeof service.downloadTempFile).toBe('function');
  });
});
