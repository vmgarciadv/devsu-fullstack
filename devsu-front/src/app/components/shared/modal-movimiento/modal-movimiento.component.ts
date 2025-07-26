import { Component, EventEmitter, Input, Output, OnInit, OnChanges, SimpleChanges } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { Movimiento } from '../../../models/movimiento.model';
import { Cuenta } from '../../../models/cuenta.model';
import { CuentasService } from '../../../services/cuentas.service';

@Component({
  selector: 'app-modal-movimiento',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './modal-movimiento.component.html',
  styleUrls: ['./modal-movimiento.component.scss']
})
export class ModalMovimientoComponent implements OnInit, OnChanges {
  @Input() isOpen: boolean = false;
  @Output() closeModal = new EventEmitter<void>();
  @Output() saveMovimiento = new EventEmitter<any>();

  movimiento = {
    tipoMovimiento: '',
    valor: null as number | null,
    numeroCuenta: ''
  };

  cuentas: Cuenta[] = [];
  activeCuentas: Cuenta[] = [];

  touched = {
    tipoMovimiento: false,
    valor: false,
    numeroCuenta: false
  };

  errors: {
    tipoMovimiento?: string;
    valor?: string;
    numeroCuenta?: string;
  } = {};

  constructor(private cuentasService: CuentasService) {}

  ngOnInit(): void {
    this.loadCuentas();
  }

  ngOnChanges(changes: SimpleChanges): void {
    if (changes['isOpen'] && this.isOpen) {
      this.resetForm();
    }
  }

  loadCuentas(): void {
    this.cuentasService.getAllCuentas().subscribe({
      next: (cuentas) => {
        this.cuentas = cuentas;
        // Filtrar cuentas activas
        this.activeCuentas = cuentas.filter(cuenta => cuenta.Estado === true);
      },
      error: (error) => {
        console.error('Error al cargar cuentas:', error);
      }
    });
  }

  onClose(): void {
    this.closeModal.emit();
    this.resetForm();
  }

  onSave(): void {
    // Validar todos los campos
    (Object.keys(this.touched) as Array<keyof typeof this.touched>).forEach(field => {
      this.touched[field] = true;
      this.validateField(field);
    });
    
    if (this.isFormValid()) {
      const dataToSend = {
        tipoMovimiento: this.movimiento.tipoMovimiento,
        valor: Number(this.movimiento.valor),
        numeroCuenta: this.movimiento.numeroCuenta
      };
      
      this.saveMovimiento.emit(dataToSend);
    }
  }

  onFieldChange(field: keyof typeof this.touched): void {
    this.touched[field] = true;
    this.validateField(field);
  }

  validateField(field: keyof typeof this.touched): void {
    this.errors[field] = '';

    switch (field) {
      case 'tipoMovimiento':
        if (!this.movimiento.tipoMovimiento) {
          this.errors.tipoMovimiento = 'El tipo de movimiento es requerido';
        }
        break;

      case 'valor':
        if (!this.movimiento.valor || this.movimiento.valor <= 0) {
          this.errors.valor = 'El valor debe ser mayor a 0';
        }
        break;

      case 'numeroCuenta':
        if (!this.movimiento.numeroCuenta.trim()) {
          this.errors.numeroCuenta = 'La cuenta es requerida';
        }
        break;
    }
  }

  isFormValid(): boolean {
    const hasErrors = Object.entries(this.errors).some(([field, error]) => {
      return this.touched[field as keyof typeof this.touched] && error;
    });
    
    return !hasErrors &&
           this.movimiento.tipoMovimiento !== '' &&
           this.movimiento.valor !== null &&
           this.movimiento.valor > 0 &&
           this.movimiento.numeroCuenta.trim() !== '';
  }

  private resetForm(): void {
    this.movimiento = {
      tipoMovimiento: '',
      valor: null,
      numeroCuenta: ''
    };
    
    (Object.keys(this.touched) as Array<keyof typeof this.touched>).forEach(key => {
      this.touched[key] = false;
    });
    
    this.errors = {};
  }
}