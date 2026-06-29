/* eslint-disable */
/* tslint:disable */
// @ts-nocheck
/*
 * ---------------------------------------------------------------
 * ## THIS FILE WAS GENERATED VIA SWAGGER-TYPESCRIPT-API        ##
 * ##                                                           ##
 * ## AUTHOR: acacode                                           ##
 * ## SOURCE: https://github.com/acacode/swagger-typescript-api ##
 * ---------------------------------------------------------------
 */

export interface ApiError {
  errorCode?: string | null;
  parameters?: any[] | null;
}

export interface ClubRegisterRequest {
  displayName: string | null;
  slug: string | null;
  planTierId: string | null;
  defaultLocaleId?: string | null;
}

export interface ControlModes {
  id: string | null;
  title: LocalizedText;
  description?: LocalizedText;
}

export interface EnumerationDto {
  id?: string | null;
  title?: LocalizedText;
  description?: LocalizedText;
}

export interface GlobalSettings {
  accentColor?: string | null;
  fontFamily?: string | null;
}

export interface InvitationDto {
  /** @format uuid */
  id?: string;
  /** @format uuid */
  targetProfileId?: string;
  email?: string | null;
  role?: string | null;
  /** @format date-time */
  expiresUtc?: string;
}

export interface InviteToSiteRequest {
  email: string | null;
  role: string | null;
}

export interface LocalizedText {
  da?: string | null;
  en?: string | null;
}

export interface MeResponse {
  registered: boolean;
  role?: string | null;
  /** @format uuid */
  profileId?: string | null;
  controlMode?: ControlModes;
  isInControl?: boolean;
  canEdit?: boolean;
  guardedProfileIds: string[] | null;
  /** @format int32 */
  pendingGuardianInvites?: number;
}

export interface RegisterIndividualSiteGuardianRequest {
  childDisplayName: string | null;
  slug: string | null;
  /** @format date-time */
  childDateOfBirth: string;
  defaultLocaleId?: string | null;
}

export interface RegisterIndividualSiteSelfRequest {
  displayName: string | null;
  slug: string | null;
  /** @format date-time */
  dateOfBirth: string;
  defaultLocaleId?: string | null;
  guardianEmail?: string | null;
  parentalConsentConfirmed?: boolean;
}

export type SectionData = object;

export interface SendOfficialEmailVerificationRequest {
  /** @format uuid */
  orgSiteId: string;
  /** @format uuid */
  clubOfficialId: string;
}

export interface SetCollaborationRequest {
  sharedEditing: boolean;
}

export interface SiteLayout {
  sections?: SiteSection[] | null;
}

export interface SiteResponse {
  /** @format uuid */
  id?: string;
  slug: string | null;
  displayName: string | null;
  defaultLocale: EnumerationDto;
  visibilityState: EnumerationDto;
}

export interface SiteResponsePagedResult {
  items?: SiteResponse[] | null;
  /** @format int32 */
  page?: number;
  /** @format int32 */
  pageSize?: number;
  /** @format int32 */
  totalCount?: number;
  /** @format int32 */
  totalPages?: number;
  hasPrevious?: boolean;
  hasNext?: boolean;
}

export interface SiteSection {
  /** @format uuid */
  id: string;
  /** @format int32 */
  order?: number;
  data: SectionData;
}

export interface SiteSnapshotResponse {
  /** @format uuid */
  id?: string;
  /** @format uuid */
  siteId?: string;
  layout?: SiteLayout;
  globalSettings?: GlobalSettings;
  /** @format int32 */
  version?: number;
}

export interface TransferControlRequest {
  to: string | null;
}

export type QueryParamsType = Record<string | number, any>;
export type ResponseFormat = keyof Omit<Body, "body" | "bodyUsed">;

export interface FullRequestParams extends Omit<RequestInit, "body"> {
  /** set parameter to `true` for call `securityWorker` for this request */
  secure?: boolean;
  /** request path */
  path: string;
  /** content type of request body */
  type?: ContentType;
  /** query params */
  query?: QueryParamsType;
  /** format of response (i.e. response.json() -> format: "json") */
  format?: ResponseFormat;
  /** request body */
  body?: unknown;
  /** base url */
  baseUrl?: string;
  /** request cancellation token */
  cancelToken?: CancelToken;
}

export type RequestParams = Omit<
  FullRequestParams,
  "body" | "method" | "query" | "path"
>;

export interface ApiConfig<SecurityDataType = unknown> {
  baseUrl?: string;
  baseApiParams?: Omit<RequestParams, "baseUrl" | "cancelToken" | "signal">;
  securityWorker?: (
    securityData: SecurityDataType | null,
  ) => Promise<RequestParams | void> | RequestParams | void;
  customFetch?: typeof fetch;
}

export interface HttpResponse<D extends unknown, E extends unknown = unknown>
  extends Response {
  data: D;
  error: E;
}

type CancelToken = Symbol | string | number;

export enum ContentType {
  Json = "application/json",
  JsonApi = "application/vnd.api+json",
  FormData = "multipart/form-data",
  UrlEncoded = "application/x-www-form-urlencoded",
  Text = "text/plain",
}

export class HttpClient<SecurityDataType = unknown> {
  public baseUrl: string = "";
  private securityData: SecurityDataType | null = null;
  private securityWorker?: ApiConfig<SecurityDataType>["securityWorker"];
  private abortControllers = new Map<CancelToken, AbortController>();
  private customFetch = (...fetchParams: Parameters<typeof fetch>) =>
    fetch(...fetchParams);

  private baseApiParams: RequestParams = {
    credentials: "same-origin",
    headers: {},
    redirect: "follow",
    referrerPolicy: "no-referrer",
  };

  constructor(apiConfig: ApiConfig<SecurityDataType> = {}) {
    Object.assign(this, apiConfig);
  }

  public setSecurityData = (data: SecurityDataType | null) => {
    this.securityData = data;
  };

  protected encodeQueryParam(key: string, value: any) {
    const encodedKey = encodeURIComponent(key);
    return `${encodedKey}=${encodeURIComponent(typeof value === "number" ? value : `${value}`)}`;
  }

  protected addQueryParam(query: QueryParamsType, key: string) {
    return this.encodeQueryParam(key, query[key]);
  }

  protected addArrayQueryParam(query: QueryParamsType, key: string) {
    const value = query[key];
    return value.map((v: any) => this.encodeQueryParam(key, v)).join("&");
  }

  protected toQueryString(rawQuery?: QueryParamsType): string {
    const query = rawQuery || {};
    const keys = Object.keys(query).filter(
      (key) => "undefined" !== typeof query[key],
    );
    return keys
      .map((key) =>
        Array.isArray(query[key])
          ? this.addArrayQueryParam(query, key)
          : this.addQueryParam(query, key),
      )
      .join("&");
  }

  protected addQueryParams(rawQuery?: QueryParamsType): string {
    const queryString = this.toQueryString(rawQuery);
    return queryString ? `?${queryString}` : "";
  }

  private contentFormatters: Record<ContentType, (input: any) => any> = {
    [ContentType.Json]: (input: any) =>
      input !== null && (typeof input === "object" || typeof input === "string")
        ? JSON.stringify(input)
        : input,
    [ContentType.JsonApi]: (input: any) =>
      input !== null && (typeof input === "object" || typeof input === "string")
        ? JSON.stringify(input)
        : input,
    [ContentType.Text]: (input: any) =>
      input !== null && typeof input !== "string"
        ? JSON.stringify(input)
        : input,
    [ContentType.FormData]: (input: any) => {
      if (input instanceof FormData) {
        return input;
      }

      return Object.keys(input || {}).reduce((formData, key) => {
        const property = input[key];
        formData.append(
          key,
          property instanceof Blob
            ? property
            : typeof property === "object" && property !== null
              ? JSON.stringify(property)
              : `${property}`,
        );
        return formData;
      }, new FormData());
    },
    [ContentType.UrlEncoded]: (input: any) => this.toQueryString(input),
  };

  protected mergeRequestParams(
    params1: RequestParams,
    params2?: RequestParams,
  ): RequestParams {
    return {
      ...this.baseApiParams,
      ...params1,
      ...(params2 || {}),
      headers: {
        ...(this.baseApiParams.headers || {}),
        ...(params1.headers || {}),
        ...((params2 && params2.headers) || {}),
      },
    };
  }

  protected createAbortSignal = (
    cancelToken: CancelToken,
  ): AbortSignal | undefined => {
    if (this.abortControllers.has(cancelToken)) {
      const abortController = this.abortControllers.get(cancelToken);
      if (abortController) {
        return abortController.signal;
      }
      return void 0;
    }

    const abortController = new AbortController();
    this.abortControllers.set(cancelToken, abortController);
    return abortController.signal;
  };

  public abortRequest = (cancelToken: CancelToken) => {
    const abortController = this.abortControllers.get(cancelToken);

    if (abortController) {
      abortController.abort();
      this.abortControllers.delete(cancelToken);
    }
  };

  public request = async <T = any, E = any>({
    body,
    secure,
    path,
    type,
    query,
    format,
    baseUrl,
    cancelToken,
    ...params
  }: FullRequestParams): Promise<HttpResponse<T, E>> => {
    const secureParams =
      ((typeof secure === "boolean" ? secure : this.baseApiParams.secure) &&
        this.securityWorker &&
        (await this.securityWorker(this.securityData))) ||
      {};
    const requestParams = this.mergeRequestParams(params, secureParams);
    const queryString = query && this.toQueryString(query);
    const payloadFormatter = this.contentFormatters[type || ContentType.Json];
    const responseFormat = format || requestParams.format;

    return this.customFetch(
      `${baseUrl || this.baseUrl || ""}${path}${queryString ? `?${queryString}` : ""}`,
      {
        ...requestParams,
        headers: {
          ...(requestParams.headers || {}),
          ...(type && type !== ContentType.FormData
            ? { "Content-Type": type }
            : {}),
        },
        signal:
          (cancelToken
            ? this.createAbortSignal(cancelToken)
            : requestParams.signal) || null,
        body:
          typeof body === "undefined" || body === null
            ? null
            : payloadFormatter(body),
      },
    ).then(async (response) => {
      const r = response as HttpResponse<T, E>;
      r.data = null as unknown as T;
      r.error = null as unknown as E;

      const responseToParse = responseFormat ? response.clone() : response;
      const data = !responseFormat
        ? r
        : await responseToParse[responseFormat]()
            .then((data) => {
              if (r.ok) {
                r.data = data;
              } else {
                r.error = data;
              }
              return r;
            })
            .catch((e) => {
              r.error = e;
              return r;
            });

      if (cancelToken) {
        this.abortControllers.delete(cancelToken);
      }

      if (!response.ok) throw data;
      return data;
    });
  };
}

/**
 * @title NextAtlet.Api
 * @version 1.0
 */
export class Api<
  SecurityDataType extends unknown,
> extends HttpClient<SecurityDataType> {
  api = {
    /**
     * No description
     *
     * @tags ActionTokens
     * @name ActionTokensAcceptCreate
     * @request POST:/api/action-tokens/{id}/accept
     * @secure
     */
    actionTokensAcceptCreate: (id: string, params: RequestParams = {}) =>
      this.request<void, ApiError>({
        path: `/api/action-tokens/${id}/accept`,
        method: "POST",
        secure: true,
        ...params,
      }),

    /**
     * No description
     *
     * @tags Clubs
     * @name ClubsScrapeCreate
     * @request POST:/api/clubs/scrape
     * @secure
     */
    clubsScrapeCreate: (
      query?: {
        /** @default "judo" */
        sport?: string;
        /** @default "denmark" */
        country?: string;
      },
      params: RequestParams = {},
    ) =>
      this.request<string, ApiError>({
        path: `/api/clubs/scrape`,
        method: "POST",
        query: query,
        secure: true,
        format: "json",
        ...params,
      }),

    /**
     * No description
     *
     * @tags Clubs
     * @name ClubsRemoveSportsUpdate
     * @request PUT:/api/clubs/remove-sports
     * @secure
     */
    clubsRemoveSportsUpdate: (
      data: string[],
      query?: {
        /** @format uuid */
        id?: string;
      },
      params: RequestParams = {},
    ) =>
      this.request<string[], ApiError>({
        path: `/api/clubs/remove-sports`,
        method: "PUT",
        query: query,
        body: data,
        secure: true,
        type: ContentType.Json,
        format: "json",
        ...params,
      }),

    /**
     * No description
     *
     * @tags Clubs
     * @name ClubsAddSportsUpdate
     * @request PUT:/api/clubs/add-sports
     * @secure
     */
    clubsAddSportsUpdate: (
      data: string[],
      query?: {
        /** @format uuid */
        id?: string;
      },
      params: RequestParams = {},
    ) =>
      this.request<string[], ApiError>({
        path: `/api/clubs/add-sports`,
        method: "PUT",
        query: query,
        body: data,
        secure: true,
        type: ContentType.Json,
        format: "json",
        ...params,
      }),

    /**
     * No description
     *
     * @tags IndividualSites
     * @name IndividualSitesSelfRegisterCreate
     * @request POST:/api/IndividualSites/self-register
     * @secure
     */
    individualSitesSelfRegisterCreate: (
      data: RegisterIndividualSiteSelfRequest,
      params: RequestParams = {},
    ) =>
      this.request<SiteResponse, ApiError>({
        path: `/api/IndividualSites/self-register`,
        method: "POST",
        body: data,
        secure: true,
        type: ContentType.Json,
        format: "json",
        ...params,
      }),

    /**
     * No description
     *
     * @tags IndividualSites
     * @name IndividualSitesGuardianRegisterCreate
     * @request POST:/api/IndividualSites/guardian-register
     * @secure
     */
    individualSitesGuardianRegisterCreate: (
      data: RegisterIndividualSiteGuardianRequest,
      params: RequestParams = {},
    ) =>
      this.request<SiteResponse, ApiError>({
        path: `/api/IndividualSites/guardian-register`,
        method: "POST",
        body: data,
        secure: true,
        type: ContentType.Json,
        format: "json",
        ...params,
      }),

    /**
     * No description
     *
     * @tags IndividualSites
     * @name IndividualSitesInviteCreate
     * @request POST:/api/IndividualSites/{id}/invite
     * @secure
     */
    individualSitesInviteCreate: (
      id: string,
      data: InviteToSiteRequest,
      params: RequestParams = {},
    ) =>
      this.request<InvitationDto, ApiError>({
        path: `/api/IndividualSites/${id}/invite`,
        method: "POST",
        body: data,
        secure: true,
        type: ContentType.Json,
        format: "json",
        ...params,
      }),

    /**
     * No description
     *
     * @tags IndividualSites
     * @name IndividualSitesTransferControlCreate
     * @request POST:/api/IndividualSites/{id}/transfer-control
     * @secure
     */
    individualSitesTransferControlCreate: (
      id: string,
      data: TransferControlRequest,
      params: RequestParams = {},
    ) =>
      this.request<void, ApiError>({
        path: `/api/IndividualSites/${id}/transfer-control`,
        method: "POST",
        body: data,
        secure: true,
        type: ContentType.Json,
        ...params,
      }),

    /**
     * No description
     *
     * @tags IndividualSites
     * @name IndividualSitesCollaborationCreate
     * @request POST:/api/IndividualSites/{id}/collaboration
     * @secure
     */
    individualSitesCollaborationCreate: (
      id: string,
      data: SetCollaborationRequest,
      params: RequestParams = {},
    ) =>
      this.request<void, ApiError>({
        path: `/api/IndividualSites/${id}/collaboration`,
        method: "POST",
        body: data,
        secure: true,
        type: ContentType.Json,
        ...params,
      }),

    /**
     * No description
     *
     * @tags IndividualSites
     * @name IndividualSitesConfigDraftList
     * @request GET:/api/IndividualSites/{id}/config/draft
     * @secure
     */
    individualSitesConfigDraftList: (id: string, params: RequestParams = {}) =>
      this.request<SiteSnapshotResponse, ApiError>({
        path: `/api/IndividualSites/${id}/config/draft`,
        method: "GET",
        secure: true,
        format: "json",
        ...params,
      }),

    /**
     * No description
     *
     * @tags Me
     * @name GetMe
     * @request GET:/api/Me
     * @secure
     */
    getMe: (params: RequestParams = {}) =>
      this.request<MeResponse, ApiError>({
        path: `/api/Me`,
        method: "GET",
        secure: true,
        format: "json",
        ...params,
      }),

    /**
     * No description
     *
     * @tags OrganizationSites
     * @name OrganizationSitesClubRegisterCreate
     * @request POST:/api/OrganizationSites/club-register
     * @secure
     */
    organizationSitesClubRegisterCreate: (
      data: ClubRegisterRequest,
      params: RequestParams = {},
    ) =>
      this.request<SiteResponse, ApiError>({
        path: `/api/OrganizationSites/club-register`,
        method: "POST",
        body: data,
        secure: true,
        type: ContentType.Json,
        format: "json",
        ...params,
      }),

    /**
     * No description
     *
     * @tags OrganizationSites
     * @name OrganizationSitesSendOfficalEmailVerificationCreate
     * @request POST:/api/OrganizationSites/send-offical-email-verification
     * @secure
     */
    organizationSitesSendOfficalEmailVerificationCreate: (
      data: SendOfficialEmailVerificationRequest,
      params: RequestParams = {},
    ) =>
      this.request<string, ApiError>({
        path: `/api/OrganizationSites/send-offical-email-verification`,
        method: "POST",
        body: data,
        secure: true,
        type: ContentType.Json,
        format: "json",
        ...params,
      }),

    /**
     * No description
     *
     * @tags Sites
     * @name SitesList
     * @request GET:/api/sites
     * @secure
     */
    sitesList: (
      query?: {
        SiteType?: string;
        Visibility?: string;
        /** @format int32 */
        Page?: number;
        /** @format int32 */
        PageSize?: number;
        SortBy?: string;
        SortDescending?: boolean;
        Search?: string;
      },
      params: RequestParams = {},
    ) =>
      this.request<SiteResponsePagedResult, ApiError>({
        path: `/api/sites`,
        method: "GET",
        query: query,
        secure: true,
        format: "json",
        ...params,
      }),
  };
}
