import { Component } from '@angular/core';
import { AvatarService } from '@app/_services/avatar.service';

@Component({
  selector: 'app-iconbyname',
  templateUrl: './iconbyname.component.html',
  styleUrls: ['./iconbyname.component.scss']
})
export class IconbynameComponent {
  params: any;
  agInit(params: any) {
    this.params = params;
  }
  constructor(private avatarService: AvatarService) { }
}
