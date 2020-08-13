import { Component } from '@angular/core';
import { SignalRService } from '../services/signalR';
import { ActivatedRoute } from '@angular/router';

@Component({
  selector: 'app-home',
  templateUrl: './home.component.html',
  styleUrls: ['./home.component.css']
})
export class HomeComponent {

  constructor(
    public sr: SignalRService,
    private route: ActivatedRoute) {
  }

  public setMyGender(num: number){
    this.sr.setUser(num)
  }

  public spinnerIsActive: boolean = false;

  public setPartnerParam(num: number){
    this.sr.searchPartner({preference: num})
    this.spinnerIsActive = true
  }

  public cancelSearch(){
    this.sr.cancelSearch()
    this.spinnerIsActive = false
  }

}
