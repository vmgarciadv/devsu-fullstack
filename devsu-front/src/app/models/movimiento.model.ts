export interface Movimiento {
  MovimientoId?: number;
  Fecha: string;
  TipoMovimiento: string;
  Valor: number;
  Saldo: number;
  NumeroCuenta: string;
  Estado?: boolean;
}