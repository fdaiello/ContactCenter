import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';

import { environment } from '@environments/environment';

import { Card } from '@app/_models/card';

@Injectable({
  providedIn: 'root'
})
export class CardService {
  
  constructor(private http: HttpClient) { }

  ngOnInit(): void { }
  /**
   * Get Current Card
   *
   * @param {*} cardId
   * @return {*} 
   * @memberof CardService
   */
  getCard(cardId) {
    return this.http.get<Card>(environment.apiUrl + '/api/Cards/' + cardId);
  }
  /**
   * Update Card
   *
   * @param {Card} card
   * @memberof CardService
   */
  updateCard(card: Card) {  
    return this.http.put<Card>(environment.apiUrl + '/api/Cards/' + card.id, card);
  }
  /**
   *
   *
   * @param {*} cardId
   * @memberof StageService
   */
  removeCard(cardId) {
    return this.http.delete<Card>(environment.apiUrl + '/api/Cards/' + cardId);
  }
  /**
   * Get All cards from given board
   *
   * @param {*} boardId
   * @return {*} 
   * @memberof CardService
   */
  getCards(boardId) {
    return this.http.get<Card[]>(environment.apiUrl + '/api/Cards?boardId=' + boardId);
  }
}
