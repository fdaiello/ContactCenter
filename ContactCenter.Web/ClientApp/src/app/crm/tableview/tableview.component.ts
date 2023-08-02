import { Component, OnInit, Input } from '@angular/core';
import { CardService } from '@app/_services/card.service';
import { MatDialog } from "@angular/material/dialog";
import { FormBuilder, FormGroup, Validators } from '@angular/forms';

import { Board } from '@app/_models/board';
import { Card } from '@app/_models/card';
import { IconbynameComponent } from '@app/crm/tableview/iconbyname/iconbyname.component';
import { CardcolorComponent } from '@app/crm/tableview/cardcolor/cardcolor.component';
import { DateformatComponent } from '@app/crm/tableview/dateformat/dateformat.component';
import { ColorFilterComponent } from '@app/crm/tableview/colorfilter/colorfilter.component';

import { Filter } from '@app/_models/filter';
import { FilterService } from '@app/_services/filter.service';
import { FilterDialogComponent } from './../dialogs/filter-dialog.component'; 
import { CustomFilterComponent } from './customfilter/customfilter.component';
import { DatePipe } from '@angular/common';

import { Stage } from '@app/_models/stage';
import { CardDialogComponent } from './../dialogs/card-dialog.component';

@Component({
  selector: 'app-tableview',
  templateUrl: './tableview.component.html',
  styleUrls: ['./tableview.component.scss']
})
export class TableviewComponent implements OnInit {

  @Input() board: Board;
  cards: Card[];
  public filters: Filter[];
  public filterSelect: FormGroup;
  public currentFilter: Filter;
  public columnMap: Map<number, number>;
  public currentStage: Stage;
  currentFilterId: number;
  
  columnDefs;
  private gridApi;
  rowSelection;
  rowData: [];
  localeText;
  AG_GRID_LOCALE_PT_BR;
  showSaveFilter;
  frameworkComponents;

  constructor(private cardService: CardService,
              private filterService: FilterService,
              private dialog: MatDialog,
              private fb: FormBuilder)
  {
    this.columnDefs = initialColumnDefs;
    this.localeText = AG_GRID_LOCALE_PT_BR;
    this.showSaveFilter = false;

    this.frameworkComponents = {
      colorFilter: ColorFilterComponent,
      customFilter: CustomFilterComponent
    };
    this.rowSelection = 'single';
  }

  ngOnInit(): void {

    this.filterSelect = this.fb.group({
      filter: [null, Validators.required]
    });

  }

  /*
   * Ao alterar o select dos Boards
   */
  ngOnChanges(): void {

    // Se temos um board selecionado
    if (this.board) {
      // Busca o conteudo
      this.getCards();
      // Busca os filtros
      this.getFilters();
    }

  }

  /*
   * Fetch cards from service - api
   */
  getCards(): void {
      // show 'loading' overlay
      if ( this.gridApi )
        this.gridApi.showLoadingOverlay();

      // card service - get cards
      this.cardService.getCards(this.board.id).subscribe(cards => {

        // Esse mapa de cartoes não sei pra que serve!
        cards.map((card, index) => {
          card.order = index;
        });

        // salva os cartões em variável do modulo
        this.cards = cards;

        // Adiciona as colunas dos campos customizados
        this.setCustomCols();

        // clear all overlays
        if ( this.gridApi)
          this.gridApi.hideOverlay();

      });
  }

  /*
   *  Acrescenta na grid os campos customizados dos cartões
   */
  setCustomCols(): void {
    if (this.cards && this.cards[0].cardFieldValues) {
      var columnDefs = this.columnDefs;
      // remove elementos inseridos anteriormente
      columnDefs.length = 8;
      // para todos os campos customizados - verifica o tipo do campo, a adiciona a coluna
      for (var x = 0; x <= this.cards[0].cardFieldValues.length - 1; x++) {
        // currency
        if (this.cards[0].cardFieldValues[x].field.fieldType == 2)
          columnDefs.push(
            { headerName: this.cards[0].cardFieldValues[x].field.label, valueFormatter: currencyFormatter, colId: this.cards[0].cardFieldValues[x].fieldId, width: 150, filter: 'customFilter', index: x }
          )

        // int or decimal
        else if (this.cards[0].cardFieldValues[x].field.fieldType == 0 || this.cards[0].cardFieldValues[x].field.fieldType == 1)
          columnDefs.push(
            {
              headerName: this.cards[0].cardFieldValues[x].field.label,
              valueFormatter: customTextFormatter,
              colId: this.cards[0].cardFieldValues[x].fieldId,
              width: 150,
              filter: 'customFilter',
              index: x
            }
          )

        // date, time
        else if (this.cards[0].cardFieldValues[x].field.fieldType == 3 || this.cards[0].cardFieldValues[x].field.fieldType == 4 )
          columnDefs.push(
            {
              headerName: this.cards[0].cardFieldValues[x].field.label,
              valueFormatter: customDateFormatter,
              sortable: true,
              filter: 'customFilter',
              colId: this.cards[0].cardFieldValues[x].fieldId,
              index: x
            }

          )

        // datetime
        else if ( this.cards[0].cardFieldValues[x].field.fieldType == 5)
          columnDefs.push(
            {
              headerName: this.cards[0].cardFieldValues[x].field.label,
              valueFormatter: customDateTimeFormatter,
              sortable: true,
              filter: 'agDateColumnFilter',
              colId: this.cards[0].cardFieldValues[x].fieldId,
              // add extra parameters for the date filter
              filterParams: {
                // provide comparator function
                comparator: (filterLocalDateAtMidnight, cellValue) => {
                  return dateComparator(filterLocalDateAtMidnight, cellValue)
                }
              },
              index: x
            }

          )

        // text area
        else if (this.cards[0].cardFieldValues[x].field.fieldType == 7)
          columnDefs.push({ headerName: this.cards[0].cardFieldValues[x].field.label, valueFormatter: customTextFormatter, colId: this.cards[0].cardFieldValues[x].fieldId, width: 450, filter: 'customFilter', index: x },)

        // demais tipos
        else
          columnDefs.push({
            headerName: this.cards[0].cardFieldValues[x].field.label,
            valueFormatter: customTextFormatter,
            colId: this.cards[0].cardFieldValues[x].fieldId,
            filter: 'customFilter',
            index: x
          })
      }
      this.gridApi.setColumnDefs(columnDefs);
    }
  }

  onGridReady(params) {
    this.gridApi = params.api;
  }

  onBtnExport() {
    window.open("\ExportBoard?boardId=" + this.board.id.toString());
  }

  onfilterChanged() {
    this.showSaveFilter = true;
  }

  onSelectionChanged(event) {
    var selectedRows = this.gridApi.getSelectedRows();
    this.currentStage = selectedRows[0].stage;
    this.openDialog(selectedRows[0].id, selectedRows[0].order)
  }

/**
 * Show Dialog and save filter when close
 *
 * @memberof tableviewComponent
 */
  onBtnSaveFilter() {

    const dialogRef = this.dialog.open(FilterDialogComponent, {
      width: "400px",
      data: {}
    });

    dialogRef.afterClosed().subscribe(result => {
      if (result) {
        let newFilter: Filter = {
          id: 0,
          boardId: this.board.id,
          title: result.name,
          jsonFilter: JSON.stringify(this.gridApi.getFilterModel())
        };
        this.filterService.createFilter(newFilter).subscribe((filter) => {
          this.getFilters();
          this.currentFilter = filter;
          this.currentFilterId = filter.id;
        });
      }
    },
    err => alert(err.error));

  }

/*
 * Carrega o select dos filtros
 */
  getFilters(): void {

    // Alimenta o select dos filtros
    this.filterService.getAllFilters(this.board.id).subscribe(filters => {
      this.filters = filters;
    },
    err => alert(err.error));

  }

/*
 *Ao selecionar um filtro
 */
  setFilter(event): void {

    // Se foi selecionado um filtro
    if (event.value > 0) {
      // Busca detalhes do filtro via api
      this.filterService.getFilter(event.value).subscribe(filter => {
        // Salva o filtro em variável publica
        this.currentFilter = filter;
        // Altera a Grid
        this.gridApi.setFilterModel(JSON.parse(filter.jsonFilter));
      },
        err => alert(err.error));
    }
    else {
      // Desativa o filtro da Grid;
      this.currentFilter = null;
      this.gridApi.setFilterModel(null);
      // para todos os campos customizados
      for (var x = 0; x <= this.cards[0].cardFieldValues.length - 1; x++) {
        this.gridApi.destroyFilter(this.cards[0].cardFieldValues[x].fieldId); // o paramentro tem que ser convertido para string, se for inteiro não funciona
      }
    }

  }

  /*
   * Exclui um filtro
   */
  deleteFilter(): void {

    this.filterService.deleteFilter(this.currentFilter.id).subscribe(
      () => {
        // Recarrega os filtros
        this.getFilters();

        // Desativa o filtro da Grid;
        this.gridApi.setFilterModel(null);
        this.currentFilter = null;
        this.currentFilterId = 0;
      },
      err => alert(err.error)
    );

  }


  /**
   * Open Dialog
   *
   * @param {Card} [card]
   * @param {number} [idx]
   * @memberof TableViewComponent
   */
  openDialog(cardId?: number, idx?: number): void {
    this.cardService.getCard(cardId).subscribe(res => {
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
          if (!result.isNew) {
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
}

// Formatador de texto para colunas customizadas do tipo Test - busca o valor do campo customizado
function customTextFormatter(params) {
  if (params.data.cardFieldValues && params.data.cardFieldValues[params.column.colDef.index]) {
    return params.data.cardFieldValues[params.column.colDef.index].value;
  }
  else {
    return "";
  }
}

// Formatador de texto para colunas customizadas do tipo Data - busca o valor do campo customizado
function customDateFormatter(params) {
  if (params.data.cardFieldValues && params.data.cardFieldValues[params.column.colDef.index]) {
    var datePipe = new DatePipe("en-US");
    return datePipe.transform(params.data.cardFieldValues[params.column.colDef.index].value, 'dd/MM/yyyy');
  }
  else {
    return "";
  }
}

// Formatador de texto para colunas customizadas do tipo Data - busca o valor do campo customizado
function customDateTimeFormatter(params) {
  if (params.data.cardFieldValues && params.data.cardFieldValues[params.column.colDef.index]) {
    var datePipe = new DatePipe("en-US");
    return datePipe.transform(params.data.cardFieldValues[params.column.colDef.index].value, 'dd/MM/yyyy HH:mm');
  }
  else {
    return "";
  }
}

var initialColumnDefs = [
  { headerName: 'avatar', field: 'contact.pictureFileName', cellRendererFramework: IconbynameComponent, width: 75, filter: false, pinned: 'left' },
  { headerName: 'nome', field: 'contact.name', sortable: true, filter: true },
  { headerName: 'celular', field: 'contact.mobilePhone', sortable: true , filter: true},
  { headerName: 'email', field: 'contact.email', sortable: true, filter: true },
  { headerName: 'estágio', field: 'stage.name', sortable: true , filter: true },
  { headerName: 'cor', field: 'color', cellRendererFramework: CardcolorComponent, width: 75, sortable: true, filter: 'colorFilter' },
  { headerName: 'atend.', field: 'contact.applicationUser.fullName', cellRendererFramework: IconbynameComponent, width: 100, sortable: true, filter: true },
  {
    headerName: 'data', field: 'updatedDate', cellRendererFramework: DateformatComponent, sortable: true, filter: 'agDateColumnFilter',
    // add extra parameters for the date filter
    filterParams: {
      // provide comparator function
      comparator: (filterLocalDateAtMidnight, cellValue) => {
        return dateComparator(filterLocalDateAtMidnight, cellValue)
      }
    }
  }
]

// Função de comparação de datas usada no Filtro
// Padrão de datas YYYY-MM-DDTHHmmss
function dateComparator(filterLocalDateAtMidnight, cellValue) {
  const dateAsString = cellValue;
  if (dateAsString == null) {
    return 0;
  }

  // In the example application, dates are stored as dd/mm/yyyy
  // We create a Date object for comparison against the filter date
  const dateParts = dateAsString.split('T')[0].split('-');
  const day = Number(dateParts[2]);
  const month = Number(dateParts[1]) - 1;
  const year = Number(dateParts[0]);
  const cellDate = new Date(year, month, day);

  // Now that both parameters are Date objects, we can compare
  if (cellDate < filterLocalDateAtMidnight) {
    return -1;
  } else if (cellDate > filterLocalDateAtMidnight) {
    return 1;
  }
  return 0;
}

function currencyFormatter(params) {
  if (params.data.cardFieldValues && params.data.cardFieldValues[params.column.colDef.id] && params.data.cardFieldValues[params.column.colDef.id].value > 0)
    return formatCurrency(params.data.cardFieldValues[params.column.colDef.id].value);
  else
    return "";
}

function formatCurrency(number) {
  var formatter = new Intl.NumberFormat('pt-BR', {
    style: 'currency',
    currency: 'BRL',
    // These options are needed to round to whole numbers if that's what you want.
    //minimumFractionDigits: 0, // (this suffices for whole numbers, but will print 2500.10 as $2,500.1)
    //maximumFractionDigits: 0, // (causes 2500.99 to be printed as $2,501)
  });
  return formatter.format(number);
}

const AG_GRID_LOCALE_PT_BR = {
  // Set Filter
  selectAll: '(Selecionar Tudo)',
  selectAllSearchResults: '(Selecionar todos resultados)',
  searchOoo: 'Procurar...',
  blanks: '(Vazio)',
  noMatches: 'Nenhum encontrado',

  // Number Filter & Text Filter
  filterOoo: 'Filtro...',
  equals: 'Igual',
  notEqual: 'Diferente',
  empty: 'Escolha',

  // Number Filter
  lessThan: 'Menor que',
  greaterThan: 'Maior que',
  lessThanOrEqual: 'menor ou igual',
  greaterThanOrEqual: 'maior ou igual',
  inRange: 'No intervalo',
  inRangeStart: 'até',
  inRangeEnd: 'de',

  // Text Filter
  contains: 'Contém',
  notContains: 'Não contém',
  startsWith: 'Começa com',
  endsWith: 'Termina com',

  // Date Filter
  dateFormatOoo: 'yyyy-mm-dd',

  // Filter Conditions
  andCondition: 'E',
  orCondition: 'OU',

  // Filter Buttons
  applyFilter: 'Applicar',
  resetFilter: 'Reset',
  clearFilter: 'Limpar',
  cancelFilter: 'Cancelar',

  // Filter Titles
  textFilter: 'Filtro de Texto',
  numberFilter: 'Filtro numérico',
  dateFilter: 'Filtro de data',
  setFilter: 'Definir Filtro',

  // Side Bar
  columns: 'Columns',
  filters: 'Filters',

  // columns tool panel
  pivotMode: 'Pivot Mode',
  groups: 'Row Groups',
  rowGroupColumnsEmptyMessage: 'Drag here to set row groups',
  values: 'Values',
  valueColumnsEmptyMessage: 'Drag here to aggregate',
  pivots: 'Column Labels',
  pivotColumnsEmptyMessage: 'Drag here to set column labels',

  // Header of the Default Group Column
  group: 'Group',

  // Other
  loadingOoo: 'Carregando...',
  noRowsToShow: 'Nenhum registro encontrado',
  enabled: 'Ativo',

  // Menu
  pinColumn: 'Pin Column',
  pinLeft: 'Pin Left',
  pinRight: 'Pin Right',
  noPin: 'No Pin',
  valueAggregation: 'Value Aggregation',
  autosizeThiscolumn: 'Autosize This Column',
  autosizeAllColumns: 'Autosize All Columns',
  groupBy: 'Group by',
  ungroupBy: 'Un-Group by',
  resetColumns: 'Reset Columns',
  expandAll: 'Expand All',
  collapseAll: 'Close All',
  copy: 'Copy',
  ctrlC: 'Ctrl+C',
  copyWithHeaders: 'Copy With Headers',
  paste: 'Paste',
  ctrlV: 'Ctrl+V',
  export: 'Export',
  csvExport: 'CSV Export',
  excelExport: 'Excel Export',

  // ARIA
  ariaHidden: 'hidden',
  ariaVisible: 'visible',
  ariaChecked: 'checked',
  ariaUnchecked: 'unchecked',
  ariaIndeterminate: 'indeterminate',
  ariaColumnSelectAll: 'Toggle Select All Columns',
  ariaInputEditor: 'Input Editor',
  ariaDateFilterInput: 'Date Filter Input',
  ariaFilterInput: 'Filter Input',
  ariaFilterColumnsInput: 'Filter Columns Input',
  ariaFilterValue: 'Filter Value',
  ariaFilterFromValue: 'Filter from value',
  ariaFilterToValue: 'Filter to value',
  ariaFilteringOperator: 'Filtering Operator',
  ariaColumnToggleVisibility: 'column toggle visibility',
  ariaColumnGroupToggleVisibility: 'column group toggle visibility',
  ariaRowSelect: 'Press SPACE to select this row',
  ariaRowDeselect: 'Press SPACE to deselect this row',
  ariaRowToggleSelection: 'Press Space to toggle row selection',
  ariaRowSelectAll: 'Press Space to toggle all rows selection',
  ariaSearch: 'Search',
  ariaSearchFilterValues: 'Search filter values'
}
