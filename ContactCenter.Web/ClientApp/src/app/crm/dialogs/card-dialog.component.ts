import { Component, EventEmitter, Inject, Output } from "@angular/core";
import { MAT_DIALOG_DATA, MatDialogRef } from "@angular/material/dialog";
import { BoardField } from '@app/_models/boardField';
import { CardService } from '@app/_services/card.service';
import { faPhone, faEnvelope } from '@fortawesome/free-solid-svg-icons';

@Component({
  selector: "app-card-dialog",
  templateUrl: './card-dialog.component.html',
  styleUrls: ["./card-dialog.component.scss"]
})
export class CardDialogComponent {
  faPhone = faPhone;
  faEnvelope = faEnvelope;
  colors = ["purple", "blue", "green", "yellow", "red", "gray", "white"];
  boardFields: BoardField[];
  @Output() deleteEvent = new EventEmitter();

  constructor(
    public dialog: MatDialogRef<CardDialogComponent>,
    @Inject(MAT_DIALOG_DATA) public data: any,
    private cardService: CardService
  ) { }

  updateBoardField(data): void {
    this.boardFields[data.index] = data.boardField;
  }

  handleDelete(): void{
    this.cardService.removeCard(this.data.card.id).subscribe(result => {
      this.deleteEvent.emit(this.data.idx);
    });
    this.dialog.close();
  }
}
