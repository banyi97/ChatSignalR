import { Injectable } from '@angular/core';
import * as signalR from '@microsoft/signalr';
import { BehaviorSubject, Observable } from 'rxjs';
import { Router } from '@angular/router';

export interface Preference{
    preference: number
}

export interface Message{
    message: string
    sender: string
}

@Injectable({providedIn: 'root'})
export class SignalRService {
    constructor(
        private route: Router,
        ){
            this.startConnection()
        }
    private hubConnection: signalR.HubConnection

    private messageSubject$: BehaviorSubject<Array<Message>|null> = new BehaviorSubject<Array<Message>|null>(null)
    public message$: Observable<Array<Message>|null> = this.messageSubject$.asObservable()

    private userSubject$: BehaviorSubject<Preference|null> = new BehaviorSubject<Preference|null>(null)
    public user$: Observable<Preference|null> = this.userSubject$.asObservable()  

    public get userIsNull() {
        return this.userSubject$.value === null
    }

    public isMatchWithId(id){
        return this.hubConnection.connectionId === id
    }

    public startConnection = () => {
        this.hubConnection = 
            new signalR.HubConnectionBuilder()
                .withUrl('chat')
                .withAutomaticReconnect()
                .build();
        this.hubConnection
            .start()
            .then(() => console.log('Connection started'))
            .catch(err => console.log('Error while starting connection: ' + err))

        this.receiveMessageListener()
        this.partnerLeaveListener()
        this.openSession()
    }

    public setUser(num: number){
        if(num === 1 || num === 2){
            this.userSubject$.next({preference: num})
        }
    }

    public searchPartner(pref: Preference){
        console.log("searchPartner")
        let obj = {connectionId: this.hubConnection.connectionId, self: this.userSubject$.value, userPreference: pref}
        console.log(obj)
        this.hubConnection.invoke("searchPartner", obj).then(data =>{
            console.log(data)
        }).catch(error => { 
            console.log(error)
        })
    } 

    public sendMessage(message: string){
        this.hubConnection.invoke("sendMessage", {message: message, sender: this.hubConnection.connectionId}).then(data => {
            console.log(data)
        }).catch(error => console.log("error"))
    }

    public testsearch(){
        this.hubConnection.invoke("searchPartner", null).then(data => {
            console.log(data)
        }).catch(error => console.log("error"))
    }

    public cancelSearch(){
        this.hubConnection.invoke("CancelSearch", null).then(data => {
            console.log(data)
        }).catch(error => console.log("error"))
    }

    public closeSession(){
        this.hubConnection.invoke("closeSession")
        .then(data => {
            console.log(data)
        })
        .catch(error => console.log(error))
        .finally(() =>{
            this.messageSubject$.next = null
            this.route.navigate([''])
        })
    }

    private openSession(){
        this.hubConnection.on("openSession", (data) =>{
            console.log(data)
            this.messageSubject$.next([])
            this.route.navigate(["/chat"])
        })
    }

    private receiveMessageListener(){
        this.hubConnection.on("receiveMessage", (data)=>{
            console.log(data)
            this.messageSubject$.value.push(data)
        }), error => console.log(error)
    }

    private partnerLeaveListener(){
        this.hubConnection.on("partnerExit", (data)=>{
            console.log(data)
        }), error => console.log(error), fin => {
            this.messageSubject$.next(null)
            this.route.navigate(['/'])
        }
    }

}