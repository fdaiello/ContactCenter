import { Component, OnInit, Input } from '@angular/core';
import { Subscription } from 'rxjs';
import { CdkDragDrop, moveItemInArray } from "@angular/cdk/drag-drop";
import { MatDialog } from "@angular/material/dialog";
import { StageDialogComponent } from './../dialogs/stage-dialog.component'; 

import { Board } from '@app/_models/board';
import { Stage } from '@app/_models/stage';

import { BoardService } from '@app/_services/board.service';
import { StageService } from '@app/_services/stage.service';

@Component({
  selector: 'app-board',
  templateUrl: './board.component.html',
  styleUrls: ['./board.component.scss']
})
export class BoardComponent implements OnInit {
  @Input() board: Board;
  public stages: Stage[];
  sub: Subscription;

  constructor(private boardService: BoardService,
              private stageService: StageService, 
              private dialog: MatDialog) { }
  
  ngOnInit() {
  }

  ngOnChanges(): void {
    if (this.board) {
      this.sub = this.stageService.getStages(this.board.id).subscribe(stages => {
        this.stages = stages.sort((a:Stage, b:Stage) => {
          return a.order - b.order;
        });
      });
    }
  }

  ngOnDestroy(): void {
    if (this.sub) {
      this.sub.unsubscribe();
    }
  }

  /**
   * Drag and drop stage element
   *
   * @param {CdkDragDrop<string[]>} event
   * @memberof BoardComponent
   */
  drop(event: CdkDragDrop<string[]>) {
    moveItemInArray(this.stages, event.previousIndex, event.currentIndex);
    this.stages.map((stage, index) => {
      stage.order = index;
    })
    this.board.stages = this.stages;
    this.boardService.updateBoard(this.board.id, this.board).subscribe();
  }
  /**
   * Show Dialog and save stage when close
   *
   * @memberof BoardComponent
   */
  openStageDialog(): void {
    const dialogRef = this.dialog.open(StageDialogComponent, {
      width: "400px",
      data: {}
    });

    dialogRef.afterClosed().subscribe(result => {
      if (result) {
        let newStage: Stage = {
          id: 0,
          boardId: this.board.id,
          name: result.name,
          label: result.label,
          cards: [],
          order: this.board.stages.length
        };
        this.stageService.createStage(newStage).subscribe(stage => this.stages.push(stage));
      }
    });
  }
  /**
   * Update board when delete stage
   *
   * @param {*} stageIndex
   * @memberof BoardComponent
   */
  update(stageIndex): void {
    if (stageIndex > -1) this.stages.splice(stageIndex, 1);
  }
}
