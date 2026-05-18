import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';
import { AppMenuitem } from './app.menuitem';

@Component({
    selector: 'app-menu',
    standalone: true,
    imports: [CommonModule, AppMenuitem, RouterModule],
    template: `<ul class="layout-menu">
        @for (item of model; track item.label) {
            @if (!item.separator) {
                <li app-menuitem [item]="item" [root]="true"></li>
            } @else {
                <li class="menu-separator"></li>
            }
        }
        <!-- <li class="agent-card">
        <span class="agent-title">Agente IMMEX</span>
        <span class="agent-description">Consulta inventarios, pedimentos y cumplimiento en lenguaje natural.</span>
        <p-button label="Abrir asistente" />
    </li> -->
    </ul> `
})
export class AppMenu implements OnInit {
    model: any[] = [];

    ngOnInit() {
        this.model = [
            {
                label: 'Main',
                items: [
                    { label: 'Dashboard', icon: 'pi pi-fw pi-home', routerLink: ['/'] }
                ]
            },
            {
                label: 'Insurance',
                items: [
                    { label: 'Quotes',   icon: 'pi pi-fw pi-file',          routerLink: ['/quotes'] },
                    { label: 'Policies', icon: 'pi pi-fw pi-shield',         routerLink: ['/policies'] },
                    { label: 'Claims',   icon: 'pi pi-fw pi-exclamation-circle', routerLink: ['/claims'] }
                ]
            }
        ];
    }
}
