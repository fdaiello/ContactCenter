import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';

import { environment } from '@environments/environment';

import { Stage } from './../_models/stage';

@Injectable({
  providedIn: 'root'
})
export class StageService {
  constructor(private http: HttpClient) { }

  ngOnInit(): void { }

  /**
   * Get All Stages from board Id
   *
   * @param {*} boardId
   * @return {*} 
   * @memberof BoardService
   */
  getStages(boardId) {
    return this.http.get<Stage[]>(environment.apiUrl + '/api/Stages?boardId=' + boardId);
  }
  /**
   * Create New Stage
   *
   * @param {*} stage
   * @return {*} 
   * @memberof StageService
   */
  createStage(stage) {
    return this.http.post<Stage>(environment.apiUrl + '/api/Stages', stage);
  }
  /**
   * Get Current Stage with stage id
   *
   * @param {*} stageId
   * @return {*} 
   * @memberof StageService
   */
  getCurrentStage(stageId) {
    return this.http.get<Stage>(environment.apiUrl + '/api/Stages/' + stageId);
  }
  /**
   * Delete Selected Stage
   *
   * @param {*} stageId
   * @return {*} 
   * @memberof StageService
   */
  deleteStage(stageId) {
    return this.http.delete(environment.apiUrl + '/api/Stages/' + stageId);
  }
  /**
   * Update Stage
   *
   * @param {*} stageId
   * @param {*} stage
   * @return {*} 
   * @memberof StageService
   */
  updateStage(stageId, stage: Stage) {
    return this.http.put<Stage>(environment.apiUrl + '/api/Stages/' + stageId, stage);
  }
}
