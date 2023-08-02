import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';

import { environment } from '@environments/environment';

import { File } from '../_models/file';
import { Observable } from 'rxjs';

@Injectable({
  providedIn: 'root'
})
export class FileService {
  constructor(private http: HttpClient) { }

  ngOnInit(): void { }

  /**
   * Get file URL based on file name
   *
   * @param {*} fileName
   * @return {*} 
   * @memberof FieldService
   */
  public getFileUrl(fileName) : Observable<any> {
    return this.http.get(environment.apiUrl + '/api/Files?filename=' + fileName);
  }

  /*
   * Upload file
   */
  public upload(formData) {

    return this.http.post<File>(environment.apiUrl + '/api/Files', formData);
  }
  /*
   * Delete file
   */
  public delete(fileName:string) {

    return this.http.delete<any>(environment.apiUrl + '/api/Files?filename=' + fileName);

  }


}
