import { Component, Inject } from '@angular/core';
import { Http } from '@angular/http';

@Component({
    selector: 'oakdale',
    templateUrl: './oakdale.component.html'
})
export class OakdaleComponent {
    public notifications: PiEvent[];

    constructor(http: Http, @Inject('BASE_URL') root:string) {
        http.get(root + '/api/events/oakdale').subscribe(result => {
            this.notifications = result.json();
        });
    }
}

interface PiEvent {
    name: string;
    active: boolean;
    startTime: Date;
    endTime: Date
    value: any;
}
