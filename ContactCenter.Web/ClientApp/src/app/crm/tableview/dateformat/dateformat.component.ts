import { Component, OnInit } from '@angular/core';

@Component({
  selector: 'app-dateformat',
  templateUrl: './dateformat.component.html',
  styleUrls: ['./dateformat.component.scss']
})
export class DateformatComponent implements OnInit {

  params: any;

  agInit(params: any) {
    this.params = params;
  }

  constructor() { }

  ngOnInit(): void {


  }

}
