import { Component, Inject } from "@angular/core";
import { MAT_DIALOG_DATA, MatDialogRef } from "@angular/material/dialog";

@Component({
  selector: "app-stage-dialog",
  templateUrl: './stage-dialog.component.html',
  styles: []
})
export class StageDialogComponent {
  constructor(
    public dialogRef: MatDialogRef<StageDialogComponent>,
    @Inject(MAT_DIALOG_DATA) public data: any
  ) {}

  onNoClick() {
    this.dialogRef.close();
  }
}
