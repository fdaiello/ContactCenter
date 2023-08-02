import { NgModule, ViewChild } from "@angular/core";
import { CommonModule } from "@angular/common";
import { DragDropModule } from "@angular/cdk/drag-drop";

import { CrmRoutingModule } from "./crm-routing.module";
import { SharedModule } from "../shared/shared.module";
import { FormsModule, ReactiveFormsModule } from "@angular/forms";
import { MatDialogModule } from "@angular/material/dialog";
import { MatButtonToggleModule } from "@angular/material/button-toggle";
import { BoardListComponent } from './board-list/board-list.component';
import { BoardComponent } from './board/board.component';
import { StageComponent } from './stage/stage.component';
import { StageDialogComponent } from "./dialogs/stage-dialog.component";
import { CardDialogComponent } from "./dialogs/card-dialog.component";
import { FilterDialogComponent } from "./dialogs/filter-dialog.component";

import { UserService } from '@app/_services/user.service';
import { BoardService } from '@app/_services/board.service';
import { StageService } from '@app/_services/stage.service';
import { CardService } from '@app/_services/card.service';
import { FontAwesomeModule } from '@fortawesome/angular-fontawesome';
import { MatExpansionModule } from '@angular/material/expansion'
import { ScrollingModule } from '@angular/cdk/scrolling';
import { TableviewComponent } from './tableview/tableview.component';
import { AgGridModule } from 'ag-grid-angular';
import { IconbynameComponent } from './tableview/iconbyname/iconbyname.component';
import { CardcolorComponent } from './tableview/cardcolor/cardcolor.component';
import { FilterService } from "../_services/filter.service";
import { DateformatComponent } from './tableview/dateformat/dateformat.component';
import { ColorFilterComponent } from './tableview/colorfilter/colorfilter.component';
import { CustomFilterComponent } from './tableview/customfilter/customfilter.component';

@NgModule({
  declarations: [
    BoardComponent,
    BoardListComponent,
    StageComponent,
    StageDialogComponent,
    FilterDialogComponent,
    CardDialogComponent,
    TableviewComponent,
    IconbynameComponent,
    CardcolorComponent,
    DateformatComponent,
    ColorFilterComponent,
    CustomFilterComponent ],
  imports: [
    CommonModule,
    CrmRoutingModule,
    SharedModule,
    FormsModule,
    ReactiveFormsModule,
    DragDropModule,
    MatDialogModule,
    MatButtonToggleModule,
    FontAwesomeModule,
    MatExpansionModule,
    ScrollingModule,
    AgGridModule.withComponents([ColorFilterComponent, CustomFilterComponent])
  ],
  providers: [
      UserService,
      BoardService,
      StageService,
      CardService,
      FilterService
  ]
})
export class CrmModule {}
