
//struct S {
//    int a;
//    int* b;
//};
//
//typedef int bool;
//typedef bool bool;
//
//typedef S Struct;
//
//typedef struct {
//    bool b = 2;
//} W;
//
//typedef struct X {
//
//} X;
//
//int main() {
//
//    bool f = 0;
//    // int a = (int)b;
//
//    int b = 3;
//
//    S test1;
//    Struct test2;
//    test1.a = 2;
//    test1.b = &b;
//    printf(test1.a);
//    printf(*test1.b);
//    W test3;
//
//    // if (b == 0)
//    //     printf(b);
//    printf("Done");
//    return 0;
//}

//int pointerTest() {
//
//    int a = 5;
//    int* b;
//    b = &a;
//    int* c = b;
//    int* d = &a;
//    int** e = &b;
//    int f = *b;
//    int g = *c;
//
//    **e = 2;
//
//    printf(*(&(**e)));
//    printf("Done");
//
//    return 0;
//}

//int fac(int n) {
//    if (n == 1) return 1;
//    return n * fac(n - 1);
//}

// This was the first goal,
// to be able to properly run this function
//int fibbonachi(int n) {
//    int a = 0, b = 1;
//    for (int i = 0; i < n; i = i + 1) {
//        int c = b;
//        b = a + b;
//        a = c;
//    }
//    return a;
//}

//void print(int a, int b, int c) {
//    printf("{0}, {1}, {2}", a, b, c);
//}

//int main() {
//    for (int i = 0; i < 20; i = i + 1) {
//        if (i % 2 == 0)
//            continue;
//        if (i == 17)
//            break;
//        printf(fibbonachi(i));
//    }
//    print(1, 2, 3);
//    printf("Done");
//    return 0;
//}

//int test() {
//    return 42;
//}
//
//int forLoopTest() {
//    int f[5] = { 1, test(), 3, 4, 5 };
//    for (int i = 0; i != 5; i++) {
//        if (i == 0 && 1 == 1 || 2 - 2 == 0)
//            printf("Jay");
//        f[i] = f[i] + 1;
//    }
//    for (int i = 0; i < 5; i++) {
//        printf(f[i]);
//    }
//
//    printf("Done");
//}

//int ternaryTest() {
//
//    int b = 2;
//    int a = b == 1 ? 2 : 0;
//
//    printf(a);
//    printf("Done");
//    return 0;
//}
