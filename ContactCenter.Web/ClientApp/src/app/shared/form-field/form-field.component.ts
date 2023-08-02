import { Component, Input, Output, EventEmitter, OnInit } from '@angular/core';
import { BoardField } from '@app/_models/boardField';
import { FileService } from '@app/_services/file.service';
import { FormGroup, FormControl, Validators } from '@angular/forms';
import { HttpErrorResponse } from '@angular/common/http';

@Component({
  selector: 'app-form-field',
  templateUrl: './form-field.component.html',
  styleUrls: ['./form-field.component.scss']
})
export class FormFieldComponent implements OnInit {
  @Input() boardField: BoardField;
  @Input() index;
  @Output() boardFieldUpdate = new EventEmitter;

  myForm = new FormGroup({
    name: new FormControl('', [Validators.required, Validators.minLength(3)]),
    file: new FormControl('', [Validators.required]),
    fileSource: new FormControl('', [Validators.required])
  });

  constructor(
    private fileService: FileService
  ) { }

  get f() {
    return this.myForm.controls;
  }

  ngOnInit() {
  }

  updateValue() {
    this.boardFieldUpdate.emit({boardField: this.boardField, index: this.index});
  }

  onOpen() {
    if (this.boardField.value.startsWith("http")) {
      window.open(this.boardField.value);
    }
    else {
      this.fileService.getFileUrl(this.boardField.value).subscribe(
        (response) => {                           // Response success
          window.open(response.url);
        },
        (error) => {                              // Error
          if (error instanceof HttpErrorResponse) {
            if (error.error instanceof ErrorEvent) {
              console.error("Error Event");
            } else {
              switch (error.status) {
                case 404:      //login
                  alert("Arquivo não localizado no servidor!")
                  this.boardField.value = null;
                  break;
                case 403:     //forbidden
                  alert("Não autorizado!")
                  break;
                default:
                  alert("Ocorreu um erro: ${error.status} ${error.statusText}")
                  break;
              }
            }
          } else {
            alert("Ocorreu algum erro não esperado ao acessar o servidor");
          }
        }
      )
    }
  }

  onDelete() {
    this.fileService.delete(this.boardField.value).subscribe(result => {
      this.boardField.value = null;
      alert("Arquivo excluido"!)
    });
  }

  onFileChange(event) {
    if (event.target.files.length > 0) {
      const file = event.target.files[0];
      this.myForm.patchValue({
        fileSource: file
      });
      console.log(file);

      const formData = new FormData();
      formData.append('file', this.myForm.get('fileSource').value);

      this.fileService.upload(formData).subscribe(result => {
        this.boardField.value = result.fileName;
        alert("Arquivo recebido!");
      });
    }
  }
}
