import { Component, Inject } from '@angular/core';
import { Http } from '@angular/http';

@Component({
    selector: 'notification',
    templateUrl: './notification.component.html'
})
export class NotificationComponent {
    public notifications: PiEvent[];

    constructor(http: Http, @Inject('BASE_URL') root:string) {
        http.get(root + '/api/events/').subscribe(result => {
            this.notifications = result.json();
            
            for (let i: number = 0; i < this.notifications.length; i++) {
                let d: Date = new Date(this.notifications[i].startTime);
                let date: string = d.toLocaleDateString();
                let time: string = d.toLocaleTimeString();
                this.notifications[i].formatStartTime = date + " " + time;
            }
        });
    }
}

interface PiEvent {
    name: string;
    active: boolean;
    startTime: Date;
    formatStartTime: string;
    endTime: Date
    value: any;
}
