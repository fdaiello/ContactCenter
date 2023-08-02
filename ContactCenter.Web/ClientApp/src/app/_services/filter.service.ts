import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';

import { Filter } from './../_models/filter';
import { environment } from '@environments/environment';

@Injectable({
    providedIn: 'root'
})
export class FilterService {
    constructor(private http: HttpClient) {}
    ngOnInit(): void {
    }
    /**
     * Get All filters
     *
     * @return {*} 
     * @memberof FilterService
     */
    getAllFilters(boardId) {
      let url = environment.apiUrl + '/api/Filters';
      if (boardId)
        url += '?boardId=' + boardId;
      return this.http.get<Filter[]>(url);
    }

    /**
     * Get Single Filter from id
     *
     * @param {*} id
     * @return {*} 
     * @memberof FilterService
     */
    getFilter(id) {
        return this.http.get<Filter>(environment.apiUrl + '/api/Filters/' + id);
    }

    /*
     * PUT filter
     */
    updateFilter(id, Filter: Filter) {
        return this.http.put<Filter>(environment.apiUrl + '/api/Filters/' + id, Filter);
    }

    /*
    * POST filter
    */
    createFilter(Filter: Filter) {
      return this.http.post<Filter>(environment.apiUrl + '/api/Filters', Filter);
    }

    /*
    * DELETE filter
    */
    deleteFilter(id) {
      return this.http.delete<Filter>(environment.apiUrl + '/api/Filters/' + id);
    }
}
