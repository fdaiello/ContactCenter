import { AgFilterComponent } from "@ag-grid-community/angular";
import { IDoesFilterPassParams, IFilterParams } from "@ag-grid-community/core";
import { Component } from '@angular/core';

@Component({
  selector: 'app-customfilter',
  templateUrl: './customfilter.component.html',
  styleUrls: ['./customfilter.component.scss']
})

/*
 * Custom Filter Component
 * Usado pela Grid para montar filtro para os campos customizados
 * - por algum motivo os filtros padrão não funcionam com os campos customizados, que tem um indice - cardfieldvalue[x].value
 * - por isto foi necessário montar um filtro customizado que le os valores dos dados da linha
 */
export class CustomFilterComponent implements AgFilterComponent {
  params: IFilterParams;
  filter: string = '';
  operator: string = 'contains';
  filtermodel: any;
  column;

  agInit(params: IFilterParams): void {
    this.params = params;
    this.column = params.column;
    this.filtermodel = null;
  }

  isFilterActive(): boolean {
    return this.filter != '';
  }

  /*
   * Função usada pela grid para comparar ( linha por linha ) se os valores da linha passam pelo filtro
   */
  doesFilterPass(params: IDoesFilterPassParams): boolean {


    if (!this.filter) {
      return true;
    }
    else {

      var filter = this.filter;

      // if typed filter is number
      if (isNumber(filter)) {
        // Fix number format for comparison
        filter = filter.replace('.', '').replace(',00', '');
      }
      // if typed filter is date
      else if (isValidDate(filter)) {
        // Fix date format for comparison
        filter = convertDateString(filter);
      }

      var value = null;
      if (params.data.cardFieldValues && params.data.cardFieldValues[this.column.colDef.index])
        value = params.data.cardFieldValues[this.column.colDef.index].value;

      if (value) {
        switch (this.operator) {
          case 'contains':
            return value.toLowerCase().includes(filter.toLowerCase());

          case '!contains':
            return !value.toLowerCase().includes(filter.toLowerCase());

          case 'equals':
            return value == filter;

          case '!equals':
            return value != filter;

          case 'greater':
            return value > filter;

          case 'lower':
            return value < filter;

          default:
            return false;
        }
      }
      else
        return false;
    }

    /*
     * Indica se uma string contém um número
     */
    function isNumber(value: string | number): boolean {
      return ((value != null) &&
        (value !== '') &&
        !isNaN(Number(value.toString())));
    }

  }

  /*
   * Chamada por tableviewcomponent ( componente pai ) para obter o modelo que define a situação do filtro atual
   */
  getModel() {
    return this.filtermodel;
  }

  /*
   * Dispara quando um filtro é definido ( em tableviewcomponent )
   * Recebe o modelo do filtro previamente salvo
   * Atribui as variaveis do filtro local
   */
  setModel(model: any) {
    if (model) {
      this.operator = model.type;
      this.filter = model.filter;
    }
    else {
      this.operator = null;
      this.filter = null;
    }
    
  }

  /*
   * Dispara quando o filtro é atualizado
   * Salva em filtermodel as variáveis do filtro atual
   */
  updateFilter() {

    this.filtermodel = { "filterType": "customfilter", "type": this.operator, "filter": this.filter, "fieldId": this.column.colDef.colId};
    this.params.filterChangedCallback();

  }

}

// Validates that the input string is a valid date formatted as "mm/dd/yyyy"
function isValidDate(dateString) {
  // First check for the pattern
  if (!/^\d{1,2}\/\d{1,2}\/\d{4}$/.test(dateString) && !/^\d{1,2}\/\d{1,2}$/.test(dateString)) {
    return false;
  }

  // Parse the date parts to integers
  var parts = dateString.split("/");
  var day = parseInt(parts[0], 10);
  var month = parseInt(parts[1], 10);
  var year
  if (parts.length == 3)
    year = ("2000" + parseInt(parts[2], 10)).slice(-4);
  else
    year = new Date().getFullYear();

  // Check the ranges of month and year
  if (year < 1000 || year > 3000 || month == 0 || month > 12) {
    return false;
  }

  var monthLength = [31, 28, 31, 30, 31, 30, 31, 31, 30, 31, 30, 31];

  // Adjust for leap years
  if (year % 400 == 0 || (year % 100 != 0 && year % 4 == 0))
    monthLength[1] = 29;

  // Check the range of the day
  return day > 0 && day <= monthLength[month - 1];
}

// Converte data no format mm/dd/yyyy para yyyy-mm-dd
function convertDateString(dateString:string) {

  // Parse the date parts to integers
  var parts = dateString.split("/");
  var day = ("00" + parseInt(parts[0], 10)).slice(-2);
  var month = ("00" + parseInt(parts[1], 10)).slice(-2);
  var year;
  if ( parts.length == 3)
    year = ("2000" + parseInt(parts[2], 10)).slice(-4);
  else
    year = new Date().getFullYear();

  // build new date format
  return year + "-" + month + "-" + day

}
