export interface DashboardViewModel {
  role: string;
  widgets: DashboardWidget[];
}

export interface DashboardWidget {
  id: string;
  type: string; // 'StatCard' | 'Chart' | 'Table' | 'Banner'
  title: string;
  width: number; // 1-12
  height: number;
  order: number;
  data: any;
}

export interface StatCardData {
  value: string;
  trend: string;
  trendDirection: 'Up' | 'Down' | 'Neutral';
  description: string;
  icon: string;
}

export interface ChartData {
  labels: string[];
  values: number[];
  chartType: string;
}

export interface TableData {
  headers: string[];
  rows: string[][];
}
