export interface Reporte {
  Fecha: string;
  Cliente: string;
  NumeroCuenta: string;
  TipoCuenta: string;
  SaldoInicial: number;
  Estado: boolean;
  TotalMovimientos: number;
  SaldoDisponible: number;
}