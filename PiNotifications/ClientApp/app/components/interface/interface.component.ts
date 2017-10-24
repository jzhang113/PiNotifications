﻿import { Component, Inject } from '@angular/core';
import { Http } from '@angular/http';

@Component({
    selector: 'interface',
    templateUrl: './interface.component.html'
})
export class InterfaceComponent {
    public notifications: PiEvent[];

    constructor(http: Http, @Inject('BASE_URL') root:string) {
        http.get(root + '/api/events/interface').subscribe(result => {
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