import { Component, OnInit } from '@angular/core';

@Component({
  selector: 'app-cardcolor',
  templateUrl: './cardcolor.component.html',
  styleUrls: ['./cardcolor.component.scss']
})
export class CardcolorComponent implements OnInit {
  params: any;
  agInit(params: any) {
    this.params = params;
  }
  constructor() { }

  ngOnInit(): void {
  }

}
