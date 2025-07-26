import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { SearchBarComponent } from '../../components/shared/search-bar/search-bar.component';
import { NotificationModalComponent } from '../../components/shared/notification-modal/notification-modal.component';
import { ModalMovimientoComponent } from '../../components/shared/modal-movimiento/modal-movimiento.component';
import { MovimientosService } from '../../services/movimientos.service';
import { NotificationService } from '../../services/notification.service';
import { Movimiento } from '../../models/movimiento.model';

@Component({
  selector: 'app-movimientos',
  standalone: true,
  imports: [CommonModule, FormsModule, SearchBarComponent, NotificationModalComponent, ModalMovimientoComponent],
  templateUrl: './movimientos.component.html',
  styleUrl: './movimientos.component.scss'
})
export class MovimientosComponent implements OnInit {
  movimientos: Movimiento[] = [];
  filteredMovimientos: Movimiento[] = [];
  notificationState$;
  isModalOpen: boolean = false;
  
  // Paginacion
  currentPage: number = 1;
  pageSize: number = 15;
  totalPages: number = 0;
  totalItems: number = 0;
  
  // Busqueda
  searchQuery: string = '';
  
  // Usar Math en el template
  Math = Math;

  constructor(
    private movimientosService: MovimientosService,
    private notificationService: NotificationService
  ) {
    this.notificationState$ = this.notificationService.notification$;
  }

  ngOnInit(): void {
    this.loadMovimientos();
  }

  loadMovimientos(page: number = 1): void {
    this.movimientosService.getMovimientos(page, this.pageSize, this.searchQuery).subscribe({
      next: (response) => {
        this.movimientos = response.Data;
        this.filteredMovimientos = [...this.movimientos];
        this.currentPage = response.PageNumber;
        this.totalPages = response.TotalPages;
        this.totalItems = response.TotalRecords;
      },
      error: (error) => {
        console.error('Error al cargar movimientos:', error);
      }
    });
  }

  onSearch(searchTerm: string): void {
    this.searchQuery = searchTerm;
    this.currentPage = 1;
    this.loadMovimientos(1);
  }

  onNewMovimiento(): void {
    this.isModalOpen = true;
  }

  onCloseModal(): void {
    this.isModalOpen = false;
  }

  onSaveMovimiento(data: any): void {
    this.movimientosService.createMovimiento(data).subscribe({
      next: () => {
        this.isModalOpen = false;
        this.notificationService.showSuccess('Movimiento creado exitosamente');
        this.loadMovimientos(this.currentPage);
      },
      error: (error) => {
        this.isModalOpen = false;
        const errorMessage = error.error?.detail || error.error?.message || 'Error al crear movimiento';
        this.notificationService.showError('Error', errorMessage);
      }
    });
  }

  closeNotification(): void {
    this.notificationService.close();
  }
  
  onPageChange(page: number): void {
    if (page >= 1 && page <= this.totalPages) {
      this.loadMovimientos(page);
    }
  }
  
  getPageNumbers(): (number | string)[] {
    const pages: (number | string)[] = [];
    const maxPagesToShow = 5;
    
    if (this.totalPages <= maxPagesToShow) {
      for (let i = 1; i <= this.totalPages; i++) {
        pages.push(i);
      }
    } else {
      pages.push(1);
      
      let start = Math.max(2, this.currentPage - 1);
      let end = Math.min(this.totalPages - 1, this.currentPage + 1);
      
      if (start > 2) {
        pages.push('...');
      }
      
      for (let i = start; i <= end; i++) {
        pages.push(i);
      }
      
      if (end < this.totalPages - 1) {
        pages.push('...');
      }
      
      pages.push(this.totalPages);
    }
    
    return pages;
  }
  
  onPageSizeChange(): void {
    this.currentPage = 1;
    this.loadMovimientos(1);
  }
}
