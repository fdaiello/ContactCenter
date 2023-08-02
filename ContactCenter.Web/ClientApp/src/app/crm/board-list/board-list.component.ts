import { Component, OnInit } from '@angular/core';
import { FormBuilder, FormGroup,FormControl,Validators } from '@angular/forms';
import { BoardService } from '@app/_services/board.service';
import { Board } from '@app/_models/board';


@Component({
  selector: 'app-board-list',
  templateUrl: './board-list.component.html',
  styleUrls: ['./board-list.component.scss']
})
export class BoardListComponent implements OnInit {
  public boards: Board[];
  public currentBoard: Board;
  public boardSelect: FormGroup;
  public viewSelectedVal: string;

  constructor(private boardService: BoardService, private fb: FormBuilder) { }

  ngOnInit() {
    this.boardSelect = this.fb.group({
      board: [null, Validators.required]
    });

    this.boardService.getAllBoards().subscribe(boards => {
      this.boards = boards;
      this.boardService.getCurrentBoard(this.boards[0].id).subscribe(board => {
        this.currentBoard = board;
        this.boardSelect.get('board').setValue(this.currentBoard.id);
        this.selectView();
      });
    });
  }
  /**
   * Update Current Board when select changes
   *
   * @param {*} event
   * @memberof BoardListComponent
   */
  updateBoard(event): void {
    this.boardService.getCurrentBoard(event.value).subscribe(board => {
      this.currentBoard = board;
      this.selectView();
    })
  }

  selectView(): void {
    if (this.currentBoard.stages.length == 1) {
      this.viewSelectedVal = 'table';
    } else {
      this.viewSelectedVal = 'kanban';
    }
  }

  /*
   * Evento de troca da View
   */
  public onValChange(val: string) {
    this.viewSelectedVal = val;
  }

}
