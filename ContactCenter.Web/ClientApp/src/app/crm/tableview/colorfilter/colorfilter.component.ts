import { Component } from '@angular/core';
import { AgFilterComponent } from "@ag-grid-community/angular";
import { IDoesFilterPassParams, IFilterParams } from "@ag-grid-community/core";

@Component({
  selector: 'app-colorfilter',
  templateUrl: './colorfilter.component.html',
  styleUrls: ['./colorfilter.component.scss']
})
export class ColorFilterComponent implements AgFilterComponent {
  params: IFilterParams;
  color: string = 'all';
  colors = ["purple", "blue", "green", "yellow", "red", "gray", "white", "all"];
  filtermodel: any;

  agInit(params: IFilterParams): void {
    this.params = params;
    this.filtermodel = null;
  }

  isFilterActive(): boolean {
    return this.color != 'all'
  }

  doesFilterPass(params: IDoesFilterPassParams): boolean {
    return params.data.color == this.color || this.color=='all'
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
      this.color = model.filter;
    }
    else {
      this.color = "all";
    }

  }

  updateFilter() {
    this.filtermodel = { "filterType": "colorfilter", "type": "equals", "filter": this.color};
    this.params.filterChangedCallback();
  }

}
