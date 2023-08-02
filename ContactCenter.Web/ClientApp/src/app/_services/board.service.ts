import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';

import { Board } from './../_models/board';
import { environment } from '@environments/environment';

@Injectable({
    providedIn: 'root'
})
export class BoardService {
    constructor(private http: HttpClient) {}
    ngOnInit(): void {
    }
    /**
     * Get All boards for User
     *
     * @return {*} 
     * @memberof BoardService
     */
    getAllBoards() {
        let url = environment.apiUrl +  '/api/Boards/CurrentUser';
        return this.http.get<Board[]>(url);
    }
    /**
     * Get Single Board from id
     *
     * @param {*} id
     * @return {*} 
     * @memberof BoardService
     */
    getCurrentBoard(id) {
        return this.http.get<Board>(environment.apiUrl + '/api/Boards/' + id);
    }

    updateBoard(id, board: Board) {
        return this.http.put<Board>(environment.apiUrl + '/api/Boards/' + id, board);
    }
}
