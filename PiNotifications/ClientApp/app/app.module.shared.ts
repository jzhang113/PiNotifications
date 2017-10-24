import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { HttpModule } from '@angular/http';
import { RouterModule } from '@angular/router';

import { AppComponent } from './components/app/app.component';
import { NavMenuComponent } from './components/navmenu/navmenu.component';
import { HomeComponent } from './components/home/home.component';

import { NotificationComponent } from './components/notification/notification.component';
import { BackupGenComponent } from './components/backupgen/backupgen.component';
import { OakdaleComponent } from './components/oakdale/oakdale.component';
import { InterfaceComponent } from './components/interface/interface.component';
import { PPTagComponent } from './components/pptag/pptag.component';

@NgModule({
    bootstrap: [ AppComponent ],
    declarations: [
        AppComponent,
        NavMenuComponent,
        HomeComponent,
        NotificationComponent,
        BackupGenComponent,
        OakdaleComponent,
        InterfaceComponent,
        PPTagComponent
    ],
    imports: [
        CommonModule,
        HttpModule,
        FormsModule,
        RouterModule.forRoot([
            { path: '', redirectTo: 'home', pathMatch: 'full' },
            { path: 'home', component: HomeComponent },
            { path: 'notification', component: NotificationComponent },
            { path: 'backup', component: BackupGenComponent },
            { path: 'oakdale', component: OakdaleComponent },
            { path: 'interface', component: InterfaceComponent },
            { path: 'pptag', component: PPTagComponent },
            { path: '**', redirectTo: 'home' }
        ])
    ]
})
export class AppModuleShared {
}
