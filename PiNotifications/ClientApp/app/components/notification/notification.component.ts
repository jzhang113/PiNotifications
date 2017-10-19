import { Component, Inject } from '@angular/core';
import { Http } from '@angular/http';

@Component({
    selector: 'notification',
    templateUrl: './notification.component.html',
    styleUrls: ['./notification.component.css']
})
export class NotificationComponent {
    public notifications: PiEvent[];

    constructor(http: Http, @Inject('BASE_URL') root:string) {
        http.get(root + '/api/events').subscribe(result => {
            this.notifications = result.json();
            console.log(this.notifications);
        });
    }
}

interface Notification {
    events: PiEvent[];
}

interface PiEvent {
    name: string;
    active: boolean;
    startTime: Date;
    endTime: Date
    value: any;
}
