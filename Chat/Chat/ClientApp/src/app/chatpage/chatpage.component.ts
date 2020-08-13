import { Component } from '@angular/core';
import { SignalRService } from '../services/signalR';
import { BehaviorSubject, Observable } from 'rxjs';

@Component({
  selector: 'app-chatpage',
  templateUrl: './chatpage.component.html',
  styleUrls: ['./chatpage.component.css']
})
export class ChatPageComponent {
  constructor(private sr: SignalRService){
    //this.sr.setTestEnv()
  }

  public messbox: string = "";
  public send(){
    if(this.disableSend)
      return
    this.sr.sendMessage(this.messbox)
    this.messbox = ""
  }
  public get disableSend(){
    return this.messbox.trim() == ""
  }

  public exit(){
    console.log("exit")
  }

  public checkId(id){
    return this.sr.isMatchWithId(id)
  }

}
