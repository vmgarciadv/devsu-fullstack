import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Router, RouterLink, RouterLinkActive } from '@angular/router';

@Component({
  selector: 'app-sidebar',
  standalone: true,
  imports: [CommonModule, RouterLink, RouterLinkActive],
  template: `
    <nav class="sidebar">
      <a 
        *ngFor="let item of menuItems" 
        class="sidebar-item"
        [routerLink]="item.route"
        routerLinkActive="active">
        {{ item.label }}
      </a>
    </nav>
  `,
  styleUrls: ['./sidebar.component.scss']
})
export class SidebarComponent {
  menuItems = [
    { label: 'Clientes', route: '/clientes' },
    { label: 'Cuentas', route: '/cuentas' },
    { label: 'Movimientos', route: '/movimientos' },
    { label: 'Reportes', route: '/reportes' }
  ];
}