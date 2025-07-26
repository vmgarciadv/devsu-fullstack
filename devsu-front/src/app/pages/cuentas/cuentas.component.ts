import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { SearchBarComponent } from '../../components/shared/search-bar/search-bar.component';
import { ModalCuentaComponent } from '../../components/shared/modal-cuenta/modal-cuenta.component';
import { NotificationModalComponent } from '../../components/shared/notification-modal/notification-modal.component';
import { ConfirmationModalComponent } from '../../components/shared/confirmation-modal/confirmation-modal.component';
import { CuentasService } from '../../services/cuentas.service';
import { NotificationService } from '../../services/notification.service';
import { Cuenta } from '../../models/cuenta.model';

@Component({
  selector: 'app-cuentas',
  standalone: true,
  imports: [CommonModule, FormsModule, SearchBarComponent, ModalCuentaComponent, NotificationModalComponent, ConfirmationModalComponent],
  templateUrl: './cuentas.component.html',
  styleUrl: './cuentas.component.scss'
})
export class CuentasComponent implements OnInit {
  cuentas: Cuenta[] = [];
  filteredCuentas: Cuenta[] = [];
  isModalOpen: boolean = false;
  modalMode: 'create' | 'edit' = 'create';
  selectedCuenta: Cuenta | null = null;
  notificationState$;
  
  isConfirmationModalOpen: boolean = false;
  cuentaToDelete: Cuenta | null = null;
  
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
    private cuentasService: CuentasService,
    private notificationService: NotificationService
  ) {
    this.notificationState$ = this.notificationService.notification$;
  }

  ngOnInit(): void {
    this.loadCuentas();
  }

  loadCuentas(page: number = 1): void {
    this.cuentasService.getCuentas(page, this.pageSize, this.searchQuery).subscribe({
      next: (response) => {
        this.cuentas = response.Data;
        this.filteredCuentas = [...this.cuentas];
        this.currentPage = response.PageNumber;
        this.totalPages = response.TotalPages;
        this.totalItems = response.TotalRecords;
      },
      error: (error) => {
        console.error('Error al cargar cuentas:', error);
      }
    });
  }

  onSearch(searchTerm: string): void {
    this.searchQuery = searchTerm;
    this.currentPage = 1;
    this.loadCuentas(1);
  }

  onNewCuenta(): void {
    this.modalMode = 'create';
    this.selectedCuenta = null;
    this.isModalOpen = true;
  }

  onCloseModal(): void {
    this.isModalOpen = false;
    this.selectedCuenta = null;
  }

  onEditCuenta(cuenta: Cuenta): void {
    this.modalMode = 'edit';
    this.selectedCuenta = cuenta;
    this.isModalOpen = true;
  }

  onSaveCuenta(event: {data: any, isPartialUpdate: boolean, modifiedFields: string[]}): void {
    if (this.modalMode === 'create') {
      this.cuentasService.createCuenta(event.data).subscribe({
        next: () => {
          this.isModalOpen = false;
          this.notificationService.showSuccess('Operación exitosa');
          this.loadCuentas();
        },
        error: (error) => {
          this.isModalOpen = false;
          const errorMessage = error.error?.detail || error.error?.message || 'Error al crear cuenta';
          this.notificationService.showError('Error', errorMessage);
        }
      });
    } else if (this.modalMode === 'edit' && this.selectedCuenta) {
      const updateObservable = event.isPartialUpdate 
        ? this.cuentasService.patchCuenta(this.selectedCuenta.CuentaId!, event.data)
        : this.cuentasService.updateCuenta(this.selectedCuenta.CuentaId!, event.data);
      
      updateObservable.subscribe({
        next: () => {
          this.isModalOpen = false;
          this.notificationService.showSuccess('Cuenta actualizada correctamente');
          this.loadCuentas();
        },
        error: (error) => {
          this.isModalOpen = false;
          const errorMessage = error.error?.detail || error.error?.message || 'Error al actualizar cuenta';
          this.notificationService.showError('Error', errorMessage);
        }
      });
    }
  }

  closeNotification(): void {
    this.notificationService.close();
  }

  onDeleteCuenta(cuenta: Cuenta): void {
    if (!cuenta.Estado) {
      this.notificationService.showError('Error', 'No se puede eliminar una cuenta inactiva');
      return;
    }
    this.cuentaToDelete = cuenta;
    this.isConfirmationModalOpen = true;
  }

  onConfirmDelete(): void {
    if (this.cuentaToDelete && this.cuentaToDelete.NumeroCuenta) {
      const numeroCuenta = typeof this.cuentaToDelete.NumeroCuenta === 'string' 
        ? parseInt(this.cuentaToDelete.NumeroCuenta) 
        : this.cuentaToDelete.NumeroCuenta;
      
      this.cuentasService.deleteCuenta(numeroCuenta).subscribe({
        next: () => {
          this.isConfirmationModalOpen = false;
          this.notificationService.showSuccess('Cuenta eliminada correctamente');
          this.loadCuentas(this.currentPage);
          this.cuentaToDelete = null;
        },
        error: (error) => {
          this.isConfirmationModalOpen = false;
          const errorMessage = error.error?.detail || error.error?.message || 'Error al eliminar cuenta';
          this.notificationService.showError('Error', errorMessage);
          this.cuentaToDelete = null;
        }
      });
    }
  }

  onCancelDelete(): void {
    this.isConfirmationModalOpen = false;
    this.cuentaToDelete = null;
  }
  
  onPageChange(page: number): void {
    if (page >= 1 && page <= this.totalPages) {
      this.loadCuentas(page);
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
    this.loadCuentas(1);
  }
}
