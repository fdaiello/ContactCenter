import { Component, Output, EventEmitter } from "@angular/core";
import { faTrashAlt } from '@fortawesome/free-solid-svg-icons';

@Component({
  selector: "app-delete-button",
  templateUrl: "./delete-button.component.html",
  styleUrls: ["./delete-button.component.scss"]
})
export class DeleteButtonComponent {
  canDelete: boolean;
  faTrashAlt = faTrashAlt;

  @Output() delete = new EventEmitter<boolean>();

  constructor() {}

  prepareForDelete() {
    this.canDelete = true;
  }

  cancel() {
    this.canDelete = false;
  }

  deleteBoard() {
    this.delete.emit(true);
    this.canDelete = false;
  }
}
