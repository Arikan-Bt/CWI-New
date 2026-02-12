export interface DashboardViewModel {
  role: string;
  widgets: DashboardWidget[];
}

export type DashboardWidgetType = 'StatCard' | 'Chart' | 'Table' | 'Banner' | 'CompositeKpi';

export interface DashboardWidget {
  id: string;
  type: DashboardWidgetType;
  title: string;
  width: number; // 1-12
  height: number;
  order: number;
  data: DashboardWidgetData;
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

export interface BannerData {
  message?: string;
  Message?: string;
}

export interface CompositeKpiData {
  sections: CompositeKpiSection[];
}

export interface CompositeKpiSection {
  title: string;
  items: CompositeKpiItem[];
}

export interface CompositeKpiItem {
  label: string;
  value: string;
  icon: string;
  trend: string;
  trendDirection: 'Up' | 'Down' | 'Neutral';
}

export type DashboardWidgetData =
  | StatCardData
  | ChartData
  | TableData
  | BannerData
  | CompositeKpiData;
