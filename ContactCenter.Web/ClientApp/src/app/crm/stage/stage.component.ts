import { Component, Input, Output, OnInit, EventEmitter } from '@angular/core';
import { CdkDragDrop, moveItemInArray, transferArrayItem } from "@angular/cdk/drag-drop";
import { MatDialog } from "@angular/material/dialog";

import { Stage } from '@app/_models/stage';
import { Card } from '@app/_models/card';

import { StageService } from '@app/_services/stage.service';
import { CardService } from '@app/_services/card.service';
import { AvatarService } from '@app/_services/avatar.service';

import { CardDialogComponent } from './../dialogs/card-dialog.component';
import { faComment } from '@fortawesome/free-solid-svg-icons';
import { environment } from '@environments/environment';

@Component({
  selector: 'app-stage',
  templateUrl: './stage.component.html',
  styleUrls: ['./stage.component.scss']
})
export class StageComponent implements OnInit {

  @Input() stage: Stage;
  @Input() stages: Stage[];
  @Input() stageIndex: Number;
  @Output() deleteEvent = new EventEmitter();
  faComment = faComment;
  apiUrl = environment.apiUrl;

  public currentStage: Stage;
  public cards: any[];
  public boardFields: any[];

  constructor(
    private stageService: StageService,
    private cardService: CardService,
    private dialog: MatDialog,
    private avatarService: AvatarService  ) { }

  ngOnInit() {

    this.stageService.getCurrentStage(this.stage.id).subscribe(result => {

      result.cards = result.cards.sort((a: Card, b: Card) => {
        return a.order - b.order;
      });
      result.cards.map((card, index) => {
        card.order = index;
      });
      this.cards = result.cards;
      this.currentStage = result;
    });
  }

  ngOnChanges(): void {
  }

  cardDrop(event: CdkDragDrop<string[]>) {
    if (event.previousContainer === event.container) {
      moveItemInArray(event.container.data, event.previousIndex, event.currentIndex);
      this.cards = this.updateCard(this.cards, event.container.data);
      this.stage.cards = this.cards;
      this.stageService.updateStage(this.stage.id, this.stage).subscribe();
    } else {
      transferArrayItem(event.previousContainer.data,
        event.container.data,
        event.previousIndex,
        event.currentIndex);
      let prevStageId = event.previousContainer.id;
      let currentStageId = event.container.id;

      let prevStage = this.updateStage(prevStageId, event.previousContainer.data);
      let currStage = this.updateStage(currentStageId, event.container.data);
      this.stageService.updateStage(prevStageId, prevStage).subscribe();
      this.stageService.updateStage(currentStageId, currStage).subscribe();
    }
  }

  /**
   * Update Stage when drag card to another stage
   *
   * @param {*} id
   * @param {*} data
   * @return {*}  {Stage}
   * @memberof StageComponent
   */
  updateStage(id, data): Stage {
    let index;
    this.stages.forEach((stage, i) => {
      if (stage.id == id) {
        stage.cards = this.updateCard(stage.cards, data);
        stage.cards.forEach(card => {
          card.stageId = stage.id;
        });
        index = i;
      }
    });
    return this.stages[index];
  }
  /**
   * Update all cards in stage when drag & drop
   *
   * @param {Card[]} cards
   * @param {any[]} data
   * @return {*}  {any[]}
   * @memberof StageComponent
   */
  updateCard(cards: Card[], data: any[]): any[] {
    cards = data;
    cards.map((card, index) => {
      card.order = index;
    });
    return cards;
  }
  /**
   * Connect Stages
   *
   * @return {*}  {any[]}
   * @memberof StageComponent
   */
  getConnectedList(): any[] {
    if (this.stages) {
      return this.stages.map(x => `${x.id}`);
    }
  }
  /**
   * Open Dialog
   *
   * @param {Card} [card]
   * @param {number} [idx]
   * @memberof StageComponent
   */
  openDialog(card?: Card, idx?: number): void {
    this.cardService.getCard(card.id).subscribe(res => {
      const dialogRef = this.dialog.open(CardDialogComponent, {
        panelClass: 'contact-center-no-padding-dialog',
        width: "500px",
        data: res
          ? { card: { ...res }, isNew: false, stageId: this.currentStage.id, idx }
          : { card: { ...res }, isNew: true }
      });
      dialogRef.componentInstance.boardFields = res.cardFieldValues;
      dialogRef.afterClosed().subscribe(result => {
        if (result) {
          if (result.isNew) {
          } else {
            const update = this.currentStage.cards;
            update.splice(result.idx, 1, result.card);
            this.cardService.updateCard(result.card).subscribe(res => {
            });
          }
        }
      });
      // Update cards after delete
      dialogRef.componentInstance.deleteEvent.subscribe((cardId: any) => {
        if (cardId > -1) this.currentStage.cards.splice(cardId, 1);
      });
    });
  }
  /**
   * Delete Stage and emit deleteEvent to Board
   *
   * @memberof StageComponent
   */
  handleDelete(): void {

    this.stageService.deleteStage(this.stage.id).subscribe(
      () => {
        this.deleteEvent.emit(this.stageIndex);
      },
      err => alert(err.error)
    );

  }
}
