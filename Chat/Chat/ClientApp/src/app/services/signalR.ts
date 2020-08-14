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

export interface UserDto{
    connectionId:string
    self: number,
    userPreference: number
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

    private partnerSubject$: BehaviorSubject<boolean> = new BehaviorSubject<boolean>(false)
    public partner$: Observable<boolean> = this.partnerSubject$.asObservable()     

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
        const dto: UserDto = {connectionId: this.hubConnection.connectionId, self: this.userSubject$.value.preference, userPreference: pref.preference}
        console.log(dto)
        this.hubConnection.invoke("searchPartner", dto).then(data =>{
            console.log(data)
        }).catch(error => { 
            console.log(error)
        })
    } 

    public sendMessage(message: string){
        console.log("sendMessage")
        this.hubConnection.invoke("sendMessage", {message: message, sender: this.hubConnection.connectionId}).then(data => {
            console.log(data)
        }).catch(error => console.log("error"))
    }

    public cancelSearch(){
        console.log("cancelSearch")
        this.hubConnection.invoke("cancel").then(data => {
            console.log(data)
        }).catch(error => console.log(error))
    }

    public closeSession(){
        console.log("closeSession")
        if(this.messageSubject$.value === null)
            return
        this.hubConnection.invoke("closeSession")
        .catch(error => console.log(error))
        .finally(() =>{
            this.messageSubject$.next(null)
            this.route.navigate([''])
        })
    }

    private openSession(){
        console.log("openSession")
        this.hubConnection.on("openSession", (data) =>{
            this.partnerSubject$.next(true)
            this.messageSubject$.next([])
            this.route.navigate(["/chat"])
        }), error => console.log(error)
    }

    private receiveMessageListener(){
        console.log("receiveMessageListener")
        this.hubConnection.on("receiveMessage", (data)=>{
            console.log(data)
            this.messageSubject$.value.push(data)
        }), error => console.log(error)
    }

    private partnerLeaveListener(){
        console.log("partnerLeaveListener")
        this.hubConnection.on("partnerExit", 
        data=>{
            this.partnerSubject$.next(false)
        }), 
        error => {
            console.log(error)
        }
    }

}