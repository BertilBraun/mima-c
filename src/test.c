
struct S {
    int a;
    int* b;
};

typedef int bool;
typedef bool bool;

typedef S Struct;

typedef struct {
    bool b = 2;
} W;

typedef struct X {

} X;

int main() {

    // bool b = 0;
    // int a = (int)b;

    S test1;
    Struct test2;
    W test2;

    // if (b == 0)
    //     printf(b);
    printf("Done");
    return 0;
}
