import { Component, EventEmitter, Input, Output } from '@angular/core';
import { FormsModule } from '@angular/forms';

@Component({
  selector: 'app-search-bar',
  standalone: true,
  imports: [FormsModule],
  template: `
    <div class="search-section">
      <div class="search-container">
        <label class="search-label" for="search">Buscar</label>
        <input 
          type="text" 
          id="search" 
          class="search-input" 
          [(ngModel)]="searchValue"
          (input)="onSearchChange()"
          [placeholder]="placeholder">
      </div>
      <button class="btn-nuevo" (click)="onNewClick()">
        Nuevo
      </button>
    </div>
  `,
  styleUrls: ['./search-bar.component.scss']
})
export class SearchBarComponent {
  @Input() placeholder = 'Buscar...';
  @Output() searchChange = new EventEmitter<string>();
  @Output() newClick = new EventEmitter<void>();

  searchValue = '';

  onSearchChange(): void {
    this.searchChange.emit(this.searchValue);
  }

  onNewClick(): void {
    this.newClick.emit();
  }
}